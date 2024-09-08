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

using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.SSOServer.Domain.Models;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal sealed class InvitationMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<Invitation>("a51c7ca1-b53e-43d2-b1ab-7efb7f5e735b", mm =>
            {
                mm.AutoMap();

                // Set members with custom serializers.
                mm.SetMemberSerializer(i => i.Emitter!, UserMap.ReferenceSerializer(dbContext));
            });
        }
    }
}
