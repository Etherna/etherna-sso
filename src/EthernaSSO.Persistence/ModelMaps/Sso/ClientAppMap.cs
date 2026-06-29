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
using Etherna.SSOServer.Domain.Models.ClientAppAgg;

namespace Etherna.SSOServer.Persistence.ModelMaps.Sso
{
    internal sealed class ClientAppMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<ClientSecret>("adf2f702-0f84-41bb-ad5d-f485372af6ef");

            dbContext.MapRegistry.AddModelMap<ClientApp>("1f980b1f-74d9-4a44-96c6-b45bfaeb6886", mm =>
            {
                mm.AutoMap();

                // Set members to ignore if null or default.
                mm.GetMemberMap(c => c.Description).SetIgnoreIfNull(true);

                // Set members with custom serializers.
                mm.SetMemberSerializer(c => c.Owner, UserMap.ReferenceSerializer(dbContext));
            });
        }
    }
}
