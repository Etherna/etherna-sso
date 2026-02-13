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

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.SystemStore
{
    internal sealed class PushedAuthorizationRequestRepository : IPushedAuthorizationRequestStore
    {
        // Fields.
        private readonly IMongoCollection<PushedAuthorizationRequest> collection;

        // Constructor.
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
        public PushedAuthorizationRequestRepository(DbContextOptions options, string name)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Register class map. (see: https://etherna.atlassian.net/browse/ESSO-140)
            BsonClassMap.RegisterClassMap<PushedAuthorizationRequest>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(x => x.ReferenceValueHash);
            });

            // Register discriminator convention. Default from MongODM doesn't work without DbContext.
            BsonSerializer.RegisterDiscriminatorConvention(typeof(PushedAuthorizationRequest),
                StandardDiscriminatorConvention.Hierarchical);

            // Initialize MongoDB driver.
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DbName);
            collection = database.GetCollection<PushedAuthorizationRequest>(name);
        }

        // Methods.
        public Task ConsumeByHashAsync(string referenceValueHash) =>
            collection.DeleteOneAsync(x => x.ReferenceValueHash == referenceValueHash);

        public async Task<PushedAuthorizationRequest?> GetByHashAsync(string referenceValueHash) =>
            await collection.AsQueryable()
                .SingleOrDefaultAsync(x => x.ReferenceValueHash == referenceValueHash);
        
        public Task StoreAsync(PushedAuthorizationRequest pushedAuthorizationRequest) =>
            collection.InsertOneAsync(pushedAuthorizationRequest);
    }
}