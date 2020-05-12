using Digicando.MongODM.HF.Tasks;
using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.SystemStore;
using Etherna.SSOServer.Persistence;
using Etherna.SSOServer.Services.EntityStores;
using Etherna.SSOServer.Services.Settings;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using Digicando.MongODM;

namespace Etherna.SSOServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions { ConnectionString = Configuration["ConnectionStrings:SystemDb"] });

            services.AddDefaultIdentity<User>(options =>
            {
                options.User.AllowedUserNameCharacters = User.AllowedUserNameCharacters;
            }).AddUserStore<UserStore>();

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);

                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                options.SlidingExpiration = true;
            });

            services.AddRazorPages();

            // Add Hangfire services.
            services.AddHangfire(config =>
            {
                config.UseMongoStorage(
                    Configuration["ConnectionStrings:HangfireDb"],
                    new MongoStorageOptions
                    {
                        MigrationOptions = new MongoMigrationOptions
                        {
                            Strategy = MongoMigrationStrategy.Migrate,
                            BackupStrategy = MongoBackupStrategy.Collections
                        }
                    });
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
            services.Configure<ApplicationSettings>(config =>
            {
                config.AssemblyVersion = appSettings.AssemblyVersion;
            });
            services.Configure<EmailSettings>(Configuration);

            // Add persistence.
            services.UseMongODM<HangfireTaskRunner>()
                .AddDbContext<ISsoDbContext, SsoDbContext>(options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:SSOServerDb"];
                    options.DocumentVersion = appSettings.SimpleAssemblyVersion;
                });

            // Configure domain.
            services.AddDomainServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
