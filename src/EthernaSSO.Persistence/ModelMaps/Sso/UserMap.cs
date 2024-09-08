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
