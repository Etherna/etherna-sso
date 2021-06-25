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

using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<User>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members to ignore if null.
                    cm.GetMemberMap(u => u.Email).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EmailConfirmed).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EtherLoginAddress).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.EtherManagedPrivateKey).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LockoutEnabled).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.Logins).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.NormalizedEmail).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.NormalizedUsername).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.PasswordHash).SetIgnoreIfNull(true);
                    cm.GetMemberMap(u => u.Username).SetIgnoreIfNull(true);

                    // Force serialization of readonly props.
                    cm.MapProperty(u => u.EtherAddress);
                });
        }
    }
}
