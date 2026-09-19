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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Options;
using Etherna.Scrinium.Core.Serialization;
using Etherna.Scrinium.Core.Serialization.Serializers;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal sealed class RoleMap : IModelMapsCollector
    {
        public void Register(IDbContextEngine dbContextEngine)
        {
            dbContextEngine.MapRegistry.AddModelMap<Role>("82413cc7-9f38-4ea2-a841-4d9479ab4f11");
        }

        /// <summary>
        /// A minimal serializer with only the id and the normalized name.
        /// </summary>
        /// <param name="originDelete">How the documents hosting the reference react when the role is deleted</param>
        public static ReferenceSerializer<Role, string> ReferenceSerializer(
            IDbContextEngine dbContextEngine,
            OriginDeleteMode originDelete) =>
            new(dbContextEngine, config =>
            {
                config.OriginDelete = originDelete;

                config.AddModelMap<ModelBase>("884090cd-f82f-48cd-973f-8c061d67f0cb");
                config.AddModelMap<EntityModelBase>("ff37854c-9437-43dc-8e4f-cc07f421e4f8", mm => { });
                config.AddModelMap<EntityModelBase<string>>("a5f3bf0d-a5f8-4574-b73a-f4637fc8ea92", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<Role>("cc9c6902-edd5-491d-acb5-07ca02fa71d0", mm =>
                {
                    mm.MapMember(m => m.NormalizedName);
                });
            });
    }
}
