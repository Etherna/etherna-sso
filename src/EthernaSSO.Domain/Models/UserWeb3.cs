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

using Etherna.SSOServer.Domain.Models.UserAgg;
using System;
using System.Linq;

namespace Etherna.SSOServer.Domain.Models
{
    public class UserWeb3 : UserBase
    {
        // Constructors.
        public UserWeb3(
            string username,
            UserBase? invitedBy,
            bool invitedByAdmin,
            UserSharedInfo sharedInfo)
            : base(username, invitedBy, invitedByAdmin, sharedInfo)
        {
            EtherAddress = sharedInfo.EtherAddress;
        }
        internal UserWeb3(UserWeb2 web2User)
        {
            ArgumentNullException.ThrowIfNull(web2User, nameof(web2User));
            if (web2User.EtherLoginAddress is null)
                throw new InvalidOperationException();

            Id = web2User.Id;
            Claims = web2User.Claims;
            CreationDateTime = web2User.CreationDateTime;
            Email = web2User.Email;
            EtherAddress = web2User.EtherLoginAddress;
            EtherPreviousAddresses = web2User.EtherPreviousAddresses.Append(web2User.EtherAddress);
            InvitedBy = web2User.InvitedBy;
            InvitedByAdmin = web2User.InvitedByAdmin;
            LastLoginDateTime = web2User.LastLoginDateTime;
            NormalizedEmail = web2User.NormalizedEmail;
            NormalizedUsername = web2User.NormalizedUsername;
            PhoneNumber = web2User.PhoneNumber;
            PhoneNumberConfirmed = web2User.PhoneNumberConfirmed;
            Roles = web2User.Roles;
            SecurityStamp = web2User.SecurityStamp;
            Username = web2User.Username;
            SharedInfoId = web2User.SharedInfoId;
        }
        protected UserWeb3() { }
    }
}
