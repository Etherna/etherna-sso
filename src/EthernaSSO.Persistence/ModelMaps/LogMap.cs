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

using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models.Logs;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    class LogMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegister.AddModelMapsSchema<LogBase>("e74be9bc-0c1f-48bf-a567-715c487d754b");
            dbContext.SchemaRegister.AddModelMapsSchema<UserLoginFailureLog>("5a52cbe4-6596-4162-88ad-e059abfa604b");
            dbContext.SchemaRegister.AddModelMapsSchema<UserLoginSuccessLog>("1178a7ac-50a1-4670-82b4-67e69b15dbc0", mm =>
            {
                mm.AutoMap();

                // Set members with custom serializers.
                mm.SetMemberSerializer(l => l.User, UserMap.ReferenceSerializer(dbContext));
            });
        }
    }
}
