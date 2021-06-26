﻿//   Copyright 2021-present Etherna Sagl
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

using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegister.AddModelMapsSchema<User>("a492aaa7-196c-4ec0-8fb5-255d099d0b9f",
                modelMap =>
                {
                    modelMap.AutoMap();

                    // Set members to ignore if null.
                    modelMap.GetMemberMap(u => u.Email).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.EmailConfirmed).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.EtherLoginAddress).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.EtherManagedPrivateKey).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.LockoutEnabled).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.Logins).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.NormalizedEmail).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.NormalizedUsername).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.PasswordHash).SetIgnoreIfNull(true);
                    modelMap.GetMemberMap(u => u.Username).SetIgnoreIfNull(true);

                    // Force serialization of readonly props.
                    modelMap.MapProperty(u => u.EtherAddress);
                });
        }
    }
}
