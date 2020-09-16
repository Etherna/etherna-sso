using Etherna.MongODM;
using Etherna.MongODM.HF.Tasks;
using Etherna.SSOServer.DataProtectionStore;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.IdentityStores;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Hangfire;
using Etherna.SSOServer.Identity;
using Etherna.SSOServer.IdentityServer;
using Etherna.SSOServer.Persistence;
using Etherna.SSOServer.Services.Settings;
using Etherna.SSOServer.Swagger;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.SSOServer
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions { ConnectionString = Configuration["ConnectionStrings:SystemDb"] });

            services.AddDefaultIdentity<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;

                //options.User.AllowedUserNameCharacters = ""; //overrided by regex validation with User.UsernameRegex
                options.User.RequireUniqueEmail = true;
            })
                .AddUserStore<UserStore>();
            //replace default UserValidator with custom. Default one doesn't allow null usernames
            services.Replace(ServiceDescriptor.Scoped<IUserValidator<User>, CustomUserValidator>());

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings.
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = Configuration["Application:CompactName"];
                options.ExpireTimeSpan = TimeSpan.FromDays(30);

                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                options.SlidingExpiration = true;

                // Response 401 for unauthorized call on api.
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    else
                        context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddCors();
            services.AddRazorPages();
            services.AddControllers(); //used for APIs
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            // Configure authentication.
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:ClientId"];
                    options.AppSecret = Configuration["Authentication:Facebook:ClientSecret"];
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = Configuration["Authentication:Twitter:ClientId"];
                    options.ConsumerSecret = Configuration["Authentication:Twitter:ClientSecret"];
                });

            // Configure IdentityServer.
            var idServerConfig = new IdServerConfig(Configuration);
            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.ErrorUrl = "/Error";
            })
                .AddInMemoryApiScopes(idServerConfig.ApiScopes)
                .AddInMemoryClients(idServerConfig.Clients)
                .AddInMemoryIdentityResources(idServerConfig.IdResources)
                .AddAspNetIdentity<User>();
            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                builder.AddSigningCredential(AzureKeyVaultAccessor.GetIdentityServerCertificate(Configuration));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            // Configure Hangfire services.
            services.AddHangfire(options =>
            {
                options.UseMongoStorage(Configuration["ConnectionStrings:HangfireDb"], new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions //don't remove, could throw exception
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                });
            });

            // Configure Swagger services.
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                //add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                //integrate xml comments
                var xmlFile = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Configure setting.
            var appSettings = new ApplicationSettings
            {
                AssemblyVersion = GetType()
                    .GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion!
            };
            services.Configure<ApplicationSettings>(options =>
            {
                options.AssemblyVersion = appSettings.AssemblyVersion;
            });
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<PageSettings>(options =>
            {
                options.ConfirmEmailPageArea = "Identity";
                options.ConfirmEmailPageUrl = "/Account/ConfirmEmail";
            });

            // Configure persistence.
            services.UseMongODM<HangfireTaskRunner, ModelBase>()
                .AddDbContext<ISsoDbContext, SsoDbContext>(options =>
                {
                    options.ApplicationVersion = appSettings.SimpleAssemblyVersion;
                    options.ConnectionString = Configuration["ConnectionStrings:SSOServerDb"];
                });

            // Configure domain services.
            services.AddDomainServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiProvider)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder =>
            {
                if (Environment.IsDevelopment())
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
                else
                {
                    builder.WithOrigins("https://*.etherna.io")
                           .SetIsOriginAllowedToAllowWildcardSubdomains()
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            // Add Hangfire.
            app.UseHangfireDashboard("/admin/hangfire",
                new DashboardOptions
                {
                    Authorization = new[] { new AdminAuthFilter() }
                });
            if (!Environment.IsStaging()) //don't start server in staging
                app.UseHangfireServer(new BackgroundJobServerOptions
                {
                    Queues = new[]
                    {
                        MongODM.Tasks.Queues.DB_MAINTENANCE,
                        "default"
                    },
                    WorkerCount = System.Environment.ProcessorCount * 2
                });

            // Add Swagger.
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in apiProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            // Add endpoints.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
