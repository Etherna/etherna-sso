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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    class RoleMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<Role>("82413cc7-9f38-4ea2-a841-4d9479ab4f11");
        }

        /// <summary>
        /// A minimal serialized with only id and normalized name
        /// </summary>
        public static ReferenceSerializer<Role, string> ReferenceSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new(dbContext, config =>
            {
                config.UseCascadeDelete = useCascadeDelete;
                config.AddModelMapsSchema<ModelBase>("884090cd-f82f-48cd-973f-8c061d67f0cb");
                config.AddModelMapsSchema<EntityModelBase>("ff37854c-9437-43dc-8e4f-cc07f421e4f8", mm => { });
                config.AddModelMapsSchema<EntityModelBase<string>>("a5f3bf0d-a5f8-4574-b73a-f4637fc8ea92", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<Role>("cc9c6902-edd5-491d-acb5-07ca02fa71d0", mm =>
                {
                    mm.MapMember(m => m.NormalizedName);
                });
            });
    }
}
