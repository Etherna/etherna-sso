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

using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Account = Nethereum.Web3.Accounts.Account;

namespace Etherna.SSOServer.Domain.Helpers
{
    internal static class UserWeb2Builder
    {
        // Consts.
        public const string EtherManagedPrivateKey = "e883fcbe10b59d63dc7f1bbed29dbd81f17a03fc65ea7d87461f45a6dfe76d0c";

        // Methods.
        public static UserWeb2 Build(
            string username = "testuser",
            UserBase? invitedBy = null,
            bool invitedByAdmin = false)
        {
            var account = new Account(EtherManagedPrivateKey);
            var sharedInfo = new UserSharedInfo(account.Address);
            return new UserWeb2(username, invitedBy, invitedByAdmin, EtherManagedPrivateKey, sharedInfo);
        }
    }
}
