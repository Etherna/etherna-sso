using Digicando.ExecContext.AsyncLocal;
using Digicando.MongODM;
using Digicando.MongODM.HF.Filters;
using Digicando.MongODM.HF.Tasks;
using Etherna.SSOServer.Domain;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.SSOServer.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, Action<PersistenceOptions>  configuration)
        {
            var options = new PersistenceOptions();
            configuration(options);

            // MongODM.
            services.UseMongODM<HangfireTaskRunner>();
            services.UseMongODMDbContext<ISsoDbContext, SsoDbContext>();

            services.Configure<DbContextOptions>(nameof(SsoDbContext), opts =>
            {
                opts.ConnectionString = configuration["MONGODB_CONNECTIONSTRING"];
                opts.DBName = configuration["MONGODB_DBNAME"];
                opts.DocumentVersion = configuration["MONGODB_DOCUMENTVERSION"];
            });

            // Add Hangfire filters.
            GlobalJobFilters.Filters.Add(new AsyncLocalContextHangfireFilter(AsyncLocalContext.Instance));
        }
    }
}
