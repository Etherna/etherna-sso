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

using Etherna.Authentication;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Configs.Identity
{
    public class CustomUserManager(
        IUserStore<UserBase> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<UserBase> passwordHasher,
        IEnumerable<IUserValidator<UserBase>> userValidators,
        IEnumerable<IPasswordValidator<UserBase>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<UserBase>> logger)
        : UserManager<UserBase>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
            keyNormalizer, errors, services, logger)
    {
        // Consts.
        //Sentinel provider name advertising FIDO2 as a valid second factor.
        //ASP.NET Identity gates RequiresTwoFactor on GetValidTwoFactorProvidersAsync returning a non-empty list,
        //but FIDO2 is not implemented as an IUserTwoFactorTokenProvider — its flow runs through IFido2Service.
        public const string Fido2ProviderName = "Fido2";

        // Methods.
        public override async Task<IList<string>> GetValidTwoFactorProvidersAsync(UserBase user)
        {
            var providers = await base.GetValidTwoFactorProvidersAsync(user);
            if (user is UserWeb2 { HasFido2Credentials: true })
                providers.Add(Fido2ProviderName);
            return providers;
        }

        public override string? GetUserId(ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal);

            // Fall back on the raw jwt claim, for bearer authenticated requests without .NET claim mapping.
            // Machine principals (client credentials tokens) have no subject: they resolve to null by design.
            return base.GetUserId(principal) ??
                principal.FindFirst(EthernaClaimTypes.UserId)?.Value;
        }

        public override string? GetUserName(ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal);

            // Fall back on the raw jwt claim, for bearer authenticated requests without .NET claim mapping.
            return base.GetUserName(principal) ??
                principal.FindFirst(EthernaClaimTypes.Username)?.Value;
        }
    }
}
