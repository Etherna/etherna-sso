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

using Etherna.ACR.Exceptions;
using Etherna.ACR.Settings;
using Etherna.DomainEvents;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Configs.Authorization;
using Etherna.SSOServer.Configs.Hangfire;
using Etherna.SSOServer.Configs.Identity;
using Etherna.SSOServer.Configs.IdentityServer;
using Etherna.SSOServer.Configs.Swagger;
using Etherna.SSOServer.Configs.SystemStore;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Persistence;
using Etherna.SSOServer.Persistence.Settings;
using Etherna.SSOServer.Services;
using Etherna.SSOServer.Services.Settings;
using Etherna.SSOServer.Services.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions
                {
                    ConnectionString = Configuration["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
                });

            services.AddIdentity<UserBase, Role>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider; //totp code

                options.User.RequireUniqueEmail = true;
            })
                .AddDefaultTokenProviders()
                .AddRoles<Role>()
                .AddRoleStore<RoleStore>()
                .AddUserStore<UserStore>();
            //replace default UserValidator with custom
            services.Replace(ServiceDescriptor.Scoped<IUserValidator<UserBase>, CustomUserValidator>());

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings.
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = Configuration["Application:CompactName"] ?? throw new ServiceConfigurationException();
                options.ExpireTimeSpan = TimeSpan.FromDays(30);

                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/AccessDenied";

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

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = Configuration.GetSection("ForwardedHeaders:KnownNetworks");
                if (knownNetworksConfig.Exists())
                {
                    var networks = knownNetworksConfig.Get<string[]>().Select(address =>
                    {
                        var parts = address.Split('/');
                        if (parts.Length != 2)
                            throw new ServiceConfigurationException();

                        return new IPNetwork(
                            IPAddress.Parse(parts[0]),
                            int.Parse(parts[1], CultureInfo.InvariantCulture));
                    });

                    foreach (var network in networks)
                        options.KnownNetworks.Add(network);
                }
            });

            services.AddCors();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(CommonConsts.AdminArea, "/", CommonConsts.RequireAdministratorRolePolicy);
                options.Conventions.AuthorizeAreaFolder(CommonConsts.IdentityArea, "/Account/Manage");

                options.Conventions.AuthorizeAreaPage(CommonConsts.IdentityArea, "/Account/Logout");
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
                    options.ClientId = Configuration["Authentication:Google:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"] ?? throw new ServiceConfigurationException();
                })
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:ClientId"] ?? throw new ServiceConfigurationException();
                    options.AppSecret = Configuration["Authentication:Facebook:ClientSecret"] ?? throw new ServiceConfigurationException();
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = Configuration["Authentication:Twitter:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ConsumerSecret = Configuration["Authentication:Twitter:ClientSecret"] ?? throw new ServiceConfigurationException();
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Audience = "ethernaSsoServiceInteract";
                    options.Authority = Configuration["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                });

            // Configure authorization.
            //policy and requirements
            services.AddAuthorization(options =>
            {
                //default policy
                options.DefaultPolicy = new AuthorizationPolicy(
                    new IAuthorizationRequirement[]
                    {
                        new DenyAnonymousAuthorizationRequirement(),
                        new DenyBannedAuthorizationRequirement()
                    },
                    Array.Empty<string>());

                //other policies
                options.AddPolicy(CommonConsts.RequireAdministratorRolePolicy,
                     policy =>
                     {
                         policy.RequireRole(Role.AdministratorName);
                         policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                     });

                options.AddPolicy(CommonConsts.ServiceInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { JwtBearerDefaults.AuthenticationScheme };
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "ethernaSso_userContactInfo_api");
                });
            });

            //requirement handlers
            services.AddScoped<IAuthorizationHandler, DenyBannedAuthorizationHandler>();

            // Configure IdentityServer.
            var idServerConfig = new IdServerConfig(Configuration);
            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.ErrorUrl = "/Error";
            })
                .AddInMemoryApiResources(idServerConfig.ApiResources)
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
                builder.AddSigningCredential(new X509Certificate2(
                    Configuration["SigningCredentialCertificate:Name"] ?? throw new ServiceConfigurationException(),
                    Configuration["SigningCredentialCertificate:Password"] ?? throw new ServiceConfigurationException()));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            services.AddSingleton<IPersistedGrantStore>(new PersistedGrantRepository(new DbContextOptions
            {
                ConnectionString = Configuration["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "persistedGrants"));

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
            services.Configure<ApplicationSettings>(Configuration.GetSection("Application") ?? throw new ServiceConfigurationException());
            services.PostConfigure<ApplicationSettings>(options =>
            {
                options.AssemblyVersion = assemblyVersion.Version;
            });
            services.Configure<EmailSettings>(Configuration.GetSection("Email") ?? throw new ServiceConfigurationException());
            services.Configure<SsoDbSeedSettings>(Configuration.GetSection("DbSeed") ?? throw new ServiceConfigurationException());

            // Configure persistence.
            services.AddMongODMWithHangfire(configureHangfireOptions: options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:HangfireDb"] ?? throw new ServiceConfigurationException();
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
            })
                .AddDbContext<ISsoDbContext, SsoDbContext>(sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    var seedSettings = sp.GetRequiredService<IOptions<SsoDbSeedSettings>>();
                    return new SsoDbContext(eventDispatcher, seedSettings.Value, sp);
                },
                options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:SSOServerDb"] ?? throw new ServiceConfigurationException();
                    options.DocumentSemVer.CurrentVersion = assemblyVersion.SimpleVersion;
                    options.ParentFor<ISharedDbContext>();
                })
                
                .AddDbContext<ISharedDbContext, SharedDbContext>(sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    return new SharedDbContext(eventDispatcher);
                },
                options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:ServiceSharedDb"] ?? throw new ServiceConfigurationException();
                    options.DocumentSemVer.CurrentVersion = assemblyVersion.SimpleVersion;
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
                app.UseForwardedHeaders();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
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

            // Seed db if required.
            app.UseDbContextsSeeding();

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
