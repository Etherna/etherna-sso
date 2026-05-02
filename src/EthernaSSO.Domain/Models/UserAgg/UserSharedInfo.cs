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

using Etherna.BeeNet.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    /* This model will not be encapsulated with UserBase until https://etherna.atlassian.net/browse/MODM-101 is solved.
     * After it, a full referenced inclusion can be implemented. */

    /// <summary>
    /// A class that expose <see cref="UserBase"/> information, but that needs to be saved on a different DbContext,
    /// shared with other Etherna services.
    /// </summary>
    public class UserSharedInfo : EntityModelBase<string>
    {
        // Fields.
        private List<EthAddress> _etherPreviousAddresses = new();

        // Constructors.
        public UserSharedInfo(EthAddress etherAddress)
        {
            EtherAddress = etherAddress;
        }
        protected UserSharedInfo() { }

        // Properties.
        [PersonalData]
        public virtual EthAddress EtherAddress { get; protected internal set; }
        [PersonalData]
        public virtual IEnumerable<EthAddress> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected internal set => _etherPreviousAddresses = [..value ?? []];
        }
        public virtual bool LockoutEnabled { get; set; }
        public virtual DateTimeOffset? LockoutEnd { get; set; }
    }
}
