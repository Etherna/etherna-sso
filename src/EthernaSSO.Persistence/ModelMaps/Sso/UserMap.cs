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
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal sealed class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<UserBase>("a492aaa7-196c-4ec0-8fb5-255d099d0b9f", mm =>
            {
                mm.AutoMap();

                // Set members to ignore if null or default.
                mm.GetMemberMap(u => u.Email).SetIgnoreIfNull(true);
                mm.GetMemberMap(u => u.InvitedBy).SetIgnoreIfNull(true);
                mm.GetMemberMap(u => u.InvitedByAdmin).SetIgnoreIfDefault(true);
                mm.GetMemberMap(u => u.NormalizedEmail).SetIgnoreIfNull(true);
                mm.GetMemberMap(u => u.NormalizedUsername).SetIgnoreIfNull(true);
                mm.GetMemberMap(u => u.Username).SetIgnoreIfNull(true);

                // Set members with custom serializers.
                mm.SetMemberSerializer(u => u.InvitedBy!, ReferenceSerializer(dbContext));
                mm.SetMemberSerializer(u => u.Roles, new EnumerableSerializer<Role>(RoleMap.ReferenceSerializer(dbContext)));
            });
            dbContext.MapRegistry.AddModelMap<UserWeb2>("2ccb567f-63cc-4fb3-b66e-a51fb4ff1bfe", mm =>
            {
                mm.AutoMap();

                // Set members to ignore if null.
                mm.GetMemberMap(u => u.EtherLoginAddress).SetIgnoreIfNull(true);
                mm.GetMemberMap(u => u.EtherManagedPrivateKey).SetIgnoreIfNull(true);
                mm.GetMemberMap(u => u.PasswordHash).SetIgnoreIfNull(true);
            });
            dbContext.MapRegistry.AddModelMap<UserWeb3>("7d8804ab-217c-476a-a47f-977fe693fce3");
        }

        /// <summary>
        /// A minimal serialized with only id and Ether address
        /// </summary>
        public static ReferenceSerializer<UserBase, string> ReferenceSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("597f29ee-f2d6-40b0-a6f4-86279f72fa68");
                config.AddModelMap<EntityModelBase>("9cf5d6bf-9c4b-49e7-9826-dafc30826e10", mm => { });
                config.AddModelMap<EntityModelBase<string>>("1ab18071-641f-405a-91bd-93a2b5c1733e", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<UserBase>("834af7e2-c858-410a-b7b9-bdaf516fa215", mm => { });
                config.AddModelMap<UserWeb2>("a1976133-bb21-40af-b6de-3a0f7f7dc676", mm => { });
                config.AddModelMap<UserWeb3>("521125ff-f337-4606-81de-89dc0afb35b0", mm => { });
            });
    }
}
