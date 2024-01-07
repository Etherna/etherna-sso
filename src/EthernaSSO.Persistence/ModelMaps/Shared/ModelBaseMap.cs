// Copyright 2021-present Etherna Sa
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
using Etherna.MongoDB.Bson.Serialization.IdGenerators;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps.Shared
{
    internal sealed class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.MapRegistry.AddModelMap<ModelBase>("d517f32d-cc45-4d21-8a99-27dca658bde5");
            dbContext.MapRegistry.AddModelMap<EntityModelBase>("4c17bb54-af84-4a21-83ae-cb1050b721f5");
            dbContext.MapRegistry.AddModelMap<EntityModelBase<string>>("e5e834e0-30cc-42a8-a1a2-9d5c79d35485",
                modelMap =>
                {
                    modelMap.AutoMap();

                    // Set Id representation.
                    modelMap.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                        .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
