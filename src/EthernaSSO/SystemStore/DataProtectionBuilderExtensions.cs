using Digicando.MongODM;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Etherna.SSOServer.SystemStore
{
    public static class DataProtectionBuilderExtensions
    {
        /// <summary>
        /// Configures the data protection system to persist keys to a MongoDb datastore
        /// </summary>
        /// <param name="builder">The <see cref="IDataProtectionBuilder"/> instance to modify.</param>
        /// <param name="dbContextOptions">Options for dbContext</param>
        /// <returns>The value <paramref name="builder"/>.</returns>
        public static IDataProtectionBuilder PersistKeysToDbContext(
            this IDataProtectionBuilder builder,
            DbContextOptions dbContextOptions)
        {
            if (builder is null)
                throw new System.ArgumentNullException(nameof(builder));

            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new XmlRepository(dbContextOptions);
            });

            return builder;
        }
    }
}
