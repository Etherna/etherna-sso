// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Etherna.Authentication.AspNetCore;
using Etherna.BeeNet.JsonConverters;
using Etherna.DomainEvents;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Etherna.SSOServer.Areas.Api;
using Etherna.SSOServer.Configs;
using Etherna.SSOServer.Configs.Authorization;
using Etherna.SSOServer.Configs.Identity;
using Etherna.SSOServer.Configs.IdentityServer;
using Etherna.SSOServer.Configs.MongODM;
using Etherna.SSOServer.Configs.OpenApi;
using Etherna.SSOServer.Configs.SystemStore;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Exceptions;
using Etherna.SSOServer.Extensions;
using Etherna.SSOServer.Middlewares.DebugPages;
using Etherna.SSOServer.Persistence;
using Etherna.SSOServer.Persistence.Settings;
using Etherna.SSOServer.Services;
using Etherna.SSOServer.Services.Options;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DashboardOptions = Etherna.MongODM.AspNetCore.UI.DashboardOptions;
using IPNetwork = System.Net.IPNetwork;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Etherna.SSOServer
{
    public static class Program
    {
        // Consts.
        private static readonly string[] StaticCorsOrigins =
        [
            "https://etherna.io",
            "https://credit.etherna.io",
            "https://gateway.etherna.io",
            "https://index.etherna.io"
        ];

        public static void Main(string[] args)
        {
            // Configure logging first.
            ConfigureLogging();

            // Then create the host, so that if the host fails we can log errors.
            try
            {
                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                // Configs.
                builder.Host.UseSerilog();

                ConfigureServices(builder);

                var app = builder.Build();
                ConfigureApplication(app);

                // First operations.
                app.SeedDbContexts();

                // Run application.
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Helpers.
        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.InvariantCulture);
            string envName = environment.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.InvariantCulture);
            return new ElasticsearchSinkOptions((configuration.GetSection("Elastic:Urls").Get<string[]>() ?? throw new ServiceConfigurationException()).Select(u => new Uri(u)))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{assemblyName}-{envName}-{DateTime.UtcNow:yyyy-MM}"
            };
        }
        
        private static void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new ServiceConfigurationException();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                .Enrich.WithProperty("Environment", environment)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var config = builder.Configuration;
            var env = builder.Environment;

            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions
                {
                    ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
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

            // Configure FIDO2 (WebAuthn).
            services.AddFido2(options => config.GetSection("Fido2").Bind(options));

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings.
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = config["Application:CompactName"] ?? throw new ServiceConfigurationException();
                options.Cookie.SecurePolicy = env.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
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

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = env.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = config.GetSection("ForwardedHeaders:KnownNetworks");
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
                        options.KnownIPNetworks.Add(network);
                }
            });

            services.AddCors();
            services.AddOpenApi("Sso03", options =>
            {
                options.AddDocumentTransformer(new SsoDocumentTransformer(
                    config["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException()));
                options.AddDocumentTransformer<MetadataFilterDocumentTransformer<SsoApiMarker>>();

                options.AddOperationTransformer<ApiMethodNeedsAuthOperationTransformer>();
                options.AddOperationTransformer<DeprecatedOperationTransformer>();
                options.AddOperationTransformer<RemoveDefaultResponse200OperationTransformer>();
                options.AddOperationTransformer<SsoOperationTransformer>();
                
                options.AddSchemaTransformer<SwarmModelsSchemaTransformer>();
                options.AddSchemaTransformer<NullableReferenceTypesSchemaTransformer>();
                options.AddDocumentTransformer<NullableStructDocumentTransformer>();
            });
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(CommonConsts.AdminArea, "/", CommonConsts.RequireAdministratorRolePolicy);
                options.Conventions.AuthorizeAreaFolder(CommonConsts.IdentityArea, "/Account/Manage");

                options.Conventions.AuthorizeAreaPage(CommonConsts.IdentityArea, "/Account/Logout");
            });
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new EthAddressJsonConverter());
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // Configure authentication.
            var allowUnsafeAuthorityConnection = false;
            if (config["IdServer:SsoServer:AllowUnsafeConnection"] is not null)
                allowUnsafeAuthorityConnection = bool.Parse(config["IdServer:SsoServer:AllowUnsafeConnection"]!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CommonConsts.UserAuthenticationPolicyScheme;
            })

                //users access
                .AddJwtBearer(CommonConsts.UserAuthenticationJwtScheme, options =>
                {
                    options.Audience = "userApi";
                    options.Authority = config["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

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
                    options.Authority = config["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                    options.ClientId = config["IdServer:SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = config["IdServer:SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

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
                    options.Authority = config["IdServer:SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                });

            // Configure authorization.
            //policy and requirements
            services.AddAuthorization(options =>
            {
                //default policy
                options.DefaultPolicy = new AuthorizationPolicy(
                    [
                        new DenyAnonymousAuthorizationRequirement(),
                        new DenyBannedAuthorizationRequirement()
                    ],
                    []);

                //other policies
                options.AddPolicy(CommonConsts.RequireAdministratorRolePolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                        policy.AddRequirements(new RequireRoleAuthorizationRequirement(
                            Role.NormalizeName(Role.AdministratorName)));
                    });
                
                options.AddPolicy(CommonConsts.UserInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { CommonConsts.UserAuthenticationJwtScheme };
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", IdServerConfig.ApiScopesDef.UserInteractEthernaSso.Name);
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
            services.AddScoped<IAuthorizationHandler, RequireRoleAuthorizationHandler>();

            // Configure IdentityServer.
            var idServerConfig = new IdServerConfig(config);
            services.AddSingleton(idServerConfig);
            services.AddIdentityServer(options =>
                {
                    options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;
                    options.Authentication.CookieSameSiteMode = SameSiteMode.Lax;
                    options.LicenseKey = config["IdServer:LicenseKey"]; //can be null in dev env
                    options.UserInteraction.ErrorUrl = "/Error";
                })
                .AddServerSideSessions()
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
                ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "persistedGrants"));
            services.AddSingleton<IPushedAuthorizationRequestStore>(new PushedAuthorizationRequestRepository(new DbContextOptions
            {
                ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "pushedAuthorizationRequests"));
            services.AddSingleton<IServerSideSessionStore>(new ServerSideSessionRepository(new DbContextOptions
            {
                ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "serverSideSessions"));
            services.AddSingleton<ISigningKeyStore>(new SigningKeyRepository(new DbContextOptions
            {
                ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
            }, "signingKeys"));

            //replace client store with composite (DB + in-memory) and CORS policy service
            var inMemoryClients = idServerConfig.Clients.ToArray();
            services.Replace(ServiceDescriptor.Scoped<IClientStore>(sp =>
                new ClientAppStore(
                    sp.GetRequiredService<ISsoDbContext>(),
                    inMemoryClients)));
            services.Replace(ServiceDescriptor.Scoped<ICorsPolicyService>(sp =>
                new CompositeCorsPolicyService(
                    sp.GetRequiredService<ISsoDbContext>(),
                    inMemoryClients)));

            // Configure Hangfire server.
            if (!env.IsStaging()) //don't start server in staging
            {
                //register hangfire server
                services.AddHangfireServer(options =>
                {
                    options.Queues =
                    [
                        Queues.DB_MAINTENANCE,
                        Queues.DOMAIN_MAINTENANCE,
                        Queues.STATS,
                        "default"
                    ];
                    options.WorkerCount = Environment.ProcessorCount * 2;
                });
            }

            // Configure setting.
            services.Configure<ApplicationOptions>(config.GetSection("Application") ?? throw new ServiceConfigurationException());
            services.Configure<EmailOptions>(config.GetSection("Email") ?? throw new ServiceConfigurationException());
            services.Configure<SsoDbSeedSettings>(config.GetSection("DbSeed") ?? throw new ServiceConfigurationException());
            
            // Configure api handler.
            services.AddScoped<ISsoApiHandler, SsoApiHandler>();

            // Configure persistence.
            services.AddMongODMWithHangfire(configureHangfireOptions: options =>
            {
                options.ConnectionString = config["ConnectionStrings:HangfireDb"] ?? throw new ServiceConfigurationException();
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
                    options.ConnectionString = config["ConnectionStrings:SSOServerDb"] ?? throw new ServiceConfigurationException();
                    options.ParentFor<ISharedDbContext>();
                })

                .AddDbContext<ISharedDbContext, SharedDbContext>(sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    return new SharedDbContext(eventDispatcher);
                },
                options =>
                {
                    options.ConnectionString = config["ConnectionStrings:ServiceSharedDb"] ?? throw new ServiceConfigurationException();
                });

            services.AddMongODMAdminDashboard(new DashboardOptions
            {
                AuthFilters = [new AdminAuthFilter()],
                BasePath = CommonConsts.DatabaseAdminPath
            });

            // Configure domain services.
            services.AddDomainServices();
        }

        private static void ConfigureApplication(WebApplication app)
        {
            var config = app.Configuration;
            var env = app.Environment;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
                app.UseEthernaDebugPages();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/StatusCode", "?code={0}");

            app.UseCors(builder =>
            {
                if (env.IsDevelopment())
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
                else
                {
                    // Allow static origins plus dynamic origins from DB-stored clients.
                    var corsPolicyService = app.Services.CreateScope().ServiceProvider
                        .GetRequiredService<ICorsPolicyService>();

                    builder.SetIsOriginAllowed(origin =>
                           {
                               // Static origins always allowed.
                               if (StaticCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                                   return true;

                               // Check dynamic origins from IdentityServer clients (DB + in-memory).
                               return corsPolicyService.IsOriginAllowedAsync(origin)
                                   .GetAwaiter().GetResult();
                           })
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseHttpMetrics();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            // Add api and pages.
            app.MapOpenApi();
            app.MapRazorPages();

            app.MapSsoApi();

            // Add Hangfire.
            app.UseHangfireDashboard(CommonConsts.HangfireAdminPath,
                new Hangfire.DashboardOptions
                {
                    Authorization = [new Configs.Hangfire.AdminAuthFilter()]
                });

            // Add Scalar API Reference.
            app.MapScalarApiReference((options, httpContext) =>
            {
                options.WithTitle("Etherna Sso API")
                    .WithOpenApiRoutePattern("/openapi/sso03.json")
                    .DisableAgent()
                    .HideClientButton()
                    .HideDeveloperTools()
                    .AddPreferredSecuritySchemes("OAuth")
                    .AddAuthorizationCodeFlow("OAuth", flow =>
                    {
                        flow.ClientId = config["IdServer:Clients:EthernaSsoScalar:ClientId"] ?? throw new ServiceConfigurationException();
                        flow.RedirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/scalar/sso03";
                        flow.Pkce = Pkce.Sha256;
                        flow.SelectedScopes = ["openid", "profile", "ether_accounts", "role", "userApi.sso"];
                    });
            });

            // Prometheus metrics scrape endpoint.
            // Access restricted at reverse proxy / network level.
            app.MapMetrics("/metrics");

            // Register cron tasks.
            RecurringJob.AddOrUpdate<ICleanupOldFailedTasksTask>(
                CleanupOldFailedTasksTask.TaskId,
                task => task.RunAsync(),
                Cron.Daily);
            
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
        }
    }
}