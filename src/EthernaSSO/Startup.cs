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

using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Etherna.ACR.Exceptions;
using Etherna.ACR.Middlewares.DebugPages;
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

            //replace default implementations with customs
            services.Replace(ServiceDescriptor.Scoped<UserManager<UserBase>, CustomUserManager>());
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
                static Task unauthorizedApiCallHandler(RedirectContext<CookieAuthenticationOptions> context)
                {
                    if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    else
                        context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                }
                options.Events.OnRedirectToAccessDenied = unauthorizedApiCallHandler;
                options.Events.OnRedirectToLogin = unauthorizedApiCallHandler;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = Configuration.GetSection("ForwardedHeaders:KnownNetworks");
                if (knownNetworksConfig.Exists())
                {
                    var networks = (knownNetworksConfig.Get<string[]>() ?? throw new ServiceConfigurationException()).Select(address =>
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
            var allowUnsafeAuthorityConnection = false;
            if (Configuration["IdServer:SsoServer:AllowUnsafeConnection"] is not null)
                allowUnsafeAuthorityConnection = bool.Parse(Configuration["IdServer:SsoServer:AllowUnsafeConnection"]!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CommonConsts.UserAuthenticationPolicyScheme;
            })

                //users access
                .AddJwtBearer(CommonConsts.UserAuthenticationJwtScheme, options =>
                {
                    options.Audience = "userApi";
                    options.Authority = Configuration["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                })
                .AddPolicyScheme(CommonConsts.UserAuthenticationPolicyScheme, CommonConsts.UserAuthenticationPolicyScheme, options =>
                {
                    //runs on each request
                    options.ForwardDefaultSelector = context =>
                    {
                        //filter by auth type
                        string? authorization = context.Request.Headers[HeaderNames.Authorization];
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            return CommonConsts.UserAuthenticationJwtScheme;

                        //otherwise always check with default cookie auth by Identity framework
                        return IdentityConstants.ApplicationScheme;
                    };
                })
                .AddEthernaOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    // Set properties.
                    options.Authority = Configuration["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                    options.ClientId = Configuration["IdServer:SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = Configuration["IdServer:SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.Scope.Add("ether_accounts");
                    options.Scope.Add("role");

                    // Handle unauthorized call on api with 401 response. For users not logged in.
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };
                })

                //services access
                .AddJwtBearer(CommonConsts.ServiceAuthenticationScheme, options =>
                {
                    options.Audience = "ethernaSsoServiceInteract";
                    options.Authority = Configuration["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
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
                         policy.RequireRole(Role.NormalizeName(Role.AdministratorName));
                         policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                     });

                options.AddPolicy(CommonConsts.ServiceInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { CommonConsts.ServiceAuthenticationScheme };
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
                options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;
                options.LicenseKey = Configuration["IdServer:LicenseKey"]; //can be null in dev env
                options.UserInteraction.ErrorUrl = "/Error";
            })
                .AddInMemoryApiResources(idServerConfig.ApiResources)
                .AddInMemoryApiScopes(idServerConfig.ApiScopes)
                .AddInMemoryClients(idServerConfig.Clients)
                .AddInMemoryIdentityResources(idServerConfig.IdResources)
                .AddAspNetIdentity<UserBase>();

            //replace default implementations with customs
            services.Replace(ServiceDescriptor.Transient<IResourceOwnerPasswordValidator, ApiKeyValidator>());

            //add other custom services
            services.AddSingleton<IPersistedGrantStore>(new PersistedGrantRepository(new DbContextOptions
            {
                ConnectionString = Configuration["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "persistedGrants"));
            services.AddSingleton<ISigningKeyStore>(new SigningKeyRepository(new DbContextOptions
            {
                ConnectionString = Configuration["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "signingKeys"));

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
                options.SupportNonNullableReferenceTypes();
                options.UseInlineDefinitionsForEnums();

                //add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                //integrate xml comments
                var xmlFile = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Configure setting.
            services.Configure<ApplicationSettings>(Configuration.GetSection("Application") ?? throw new ServiceConfigurationException());
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
                app.UseEthernaAcrDebugPages();
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
                    builder.WithOrigins("https://etherna.io")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseIdentityServer();
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
                options.DocumentTitle = "Etherna SSO API";

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
                Cron.Daily(2));

            RecurringJob.AddOrUpdate<IDeleteOldInvitationsTask>(
                DeleteOldInvitationsTask.TaskId,
                task => task.RunAsync(),
                Cron.Daily(5));

            RecurringJob.AddOrUpdate<IProcessAlphaPassRequestsTask>(
                ProcessAlphaPassRequestsTask.TaskId,
                task => task.RunAsync(),
                Cron.Hourly());

            RecurringJob.AddOrUpdate<IWeb3LoginTokensCleanTask>(
                Web3LoginTokensCleanTask.TaskId,
                task => task.RunAsync(),
                Cron.Daily(3));

            // Seed db if required.
            app.SeedDbContexts();
        }
    }
}
