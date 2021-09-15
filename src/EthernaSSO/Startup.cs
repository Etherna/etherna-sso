//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.DomainEvents;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Configs.Hangfire;
using Etherna.SSOServer.Configs.Identity;
using Etherna.SSOServer.Configs.IdentityServer;
using Etherna.SSOServer.Configs.Swagger;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Persistence;
using Etherna.SSOServer.Services.Settings;
using Etherna.SSOServer.Services.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.SSOServer
{
    public class Startup
    {
        // Constructor.
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // Properties.
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // Methods.
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions { ConnectionString = Configuration["ConnectionStrings:SystemDb"] });

            services.AddDefaultIdentity<UserBase>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;

                //options.User.AllowedUserNameCharacters = ""; //overrided by regex validation with User.UsernameRegex
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<Role>()
                .AddRoleStore<RoleStore>()
                .AddUserStore<UserStore>();
            //replace default UserValidator with custom. Default one doesn't allow null usernames
            services.Replace(ServiceDescriptor.Scoped<IUserValidator<UserBase>, CustomUserValidator>());

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
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(CommonConsts.AdminArea, "/", "RequireAdministratorRole");
            });
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

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

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
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = Configuration["IdServer:SsoServer:BaseUrl"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole(Role.AdministratorName));

                options.AddPolicy("ServiceInteractApiScope", policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { JwtBearerDefaults.AuthenticationScheme };
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "ethernaSso_userContactInfo_api");
                });
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
                .AddAspNetIdentity<UserBase>();
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

            // Configure Hangfire server.
            if (!Environment.IsStaging()) //don't start server in staging
            {
                //register hangfire server
                services.AddHangfireServer(options =>
                {
                    options.Queues = new[]
                    {
                        Queues.DB_MAINTENANCE,
                        Queues.DOMAIN_MAINTENANCE,
                        Queues.STATS,
                        "default"
                    };
                    options.WorkerCount = System.Environment.ProcessorCount * 2;
                });
            }

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
            var assemblyVersion = new AssemblyVersion(GetType().GetTypeInfo().Assembly);
            services.Configure<ApplicationSettings>(Configuration.GetSection("Application"));
            services.PostConfigure<ApplicationSettings>(options =>
            {
                options.AssemblyVersion = assemblyVersion.Version;
            });
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));

            // Configure persistence.
            services.AddMongODMWithHangfire<ModelBase>(configureHangfireOptions: options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:HangfireDb"];
                options.StorageOptions = new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions //don't remove, could throw exception
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                };
            }, configureMongODMOptions: options =>
            {
                options.DbMaintenanceQueueName = Queues.DB_MAINTENANCE;
            }).AddDbContext<ISsoDbContext, SsoDbContext>(sp =>
            {
                var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                var passwordHasher = sp.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher<UserBase>>();
                return new SsoDbContext(eventDispatcher, passwordHasher);
            },
            options =>
            {
                options.DocumentSemVer.CurrentVersion = assemblyVersion.SimpleVersion;
                options.ConnectionString = Configuration["ConnectionStrings:SSOServerDb"];
            });

            services.AddMongODMAdminDashboard(new MongODM.AspNetCore.UI.DashboardOptions
            {
                AuthFilters = new[] { new Configs.MongODM.AdminAuthFilter() },
                BasePath = CommonConsts.DatabaseAdminPath
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
            app.UseHangfireDashboard(CommonConsts.HangfireAdminPath,
                new Hangfire.DashboardOptions
                {
                    Authorization = new[] { new AdminAuthFilter() }
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

            // Register cron tasks.
            RecurringJob.AddOrUpdate<ICompileDailyStatsTask>(
                CompileDailyStatsTask.TaskId,
                task => task.RunAsync(),
                "0 2 * * *"); //at 02:00 every day

            RecurringJob.AddOrUpdate<IDeleteOldInvitationsTask>(
                DeleteOldInvitationsTask.TaskId,
                task => task.RunAsync(),
                "0 5 * * *"); //at 05:00 every day
        }
    }
}
