﻿// Copyright 2021-present Etherna Sa
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

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal sealed class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.MapRegistry.AddModelMap<ModelBase>("eff57bde-15fb-44ba-ab38-e600ad2835e2");
            dbContext.MapRegistry.AddModelMap<EntityModelBase>("d8ead46c-ba33-4587-b478-16aab83ffb47");
            dbContext.MapRegistry.AddModelMap<EntityModelBase<string>>("7ddc64f6-3459-4641-957a-7c1dce83d56e",
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
