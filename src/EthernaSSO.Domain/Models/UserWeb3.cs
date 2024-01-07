// Copyright 2021-present Etherna Sa
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
