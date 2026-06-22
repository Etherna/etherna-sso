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
using Etherna.SSOServer.Domain.Models.Fido2CredentialAgg;
using Fido2NetLib;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    public interface IFido2Service
    {
        Task<AssertionOptions> BeginAssertionAsync(UserWeb2 user, CancellationToken? cancellationToken = null);
        Task<CredentialCreateOptions> BeginRegistrationAsync(UserWeb2 user, CancellationToken? cancellationToken = null);
        Task<Fido2Credential> CompleteAssertionAsync(UserWeb2 user, AuthenticatorAssertionRawResponse assertion, CancellationToken? cancellationToken = null);
        Task<Fido2Credential> CompleteRegistrationAsync(UserWeb2 user, AuthenticatorAttestationRawResponse attestation, string nickname, CancellationToken? cancellationToken = null);
    }
}
