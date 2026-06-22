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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.Fido2CredentialAgg;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Domain
{
    internal sealed class Fido2Service(
        IFido2 fido2,
        ISsoDbContext ssoDbContext)
        : IFido2Service
    {
        // Methods.
        public async Task<AssertionOptions> BeginAssertionAsync(
            UserWeb2 user,
            CancellationToken? cancellationToken = null)
        {
            ArgumentNullException.ThrowIfNull(user);

            var allowedCredentials = user.Fido2Credentials
                .Select(c => new PublicKeyCredentialDescriptor(
                    PublicKeyCredentialType.PublicKey,
                    c.CredentialId,
                    ParseTransports(c.Transports)))
                .ToList();

            var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = allowedCredentials,
                //Used as a second factor (the password already proved knowledge), so don't request user
                //verification: it keeps roaming keys to a single touch instead of forcing a key PIN/biometric.
                UserVerification = UserVerificationRequirement.Discouraged
            });

            await PersistChallengeAsync(user, Fido2ChallengePurpose.Assertion, options.ToJson());

            return options;
        }

        public async Task<CredentialCreateOptions> BeginRegistrationAsync(
            UserWeb2 user,
            CancellationToken? cancellationToken = null)
        {
            ArgumentNullException.ThrowIfNull(user);

            var fido2User = new Fido2User
            {
                Id = Encoding.UTF8.GetBytes(user.Id),
                Name = user.Username,
                DisplayName = user.Username
            };
            var excludeCredentials = user.Fido2Credentials
                .Select(c => new PublicKeyCredentialDescriptor(
                    PublicKeyCredentialType.PublicKey,
                    c.CredentialId,
                    ParseTransports(c.Transports)))
                .ToList();

            var options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fido2User,
                ExcludeCredentials = excludeCredentials,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    AuthenticatorAttachment = null,
                    ResidentKey = ResidentKeyRequirement.Discouraged,
                    //Register as a pure second factor: no user verification requirement (see BeginAssertionAsync).
                    UserVerification = UserVerificationRequirement.Discouraged
                },
                AttestationPreference = AttestationConveyancePreference.None
            });

            await PersistChallengeAsync(user, Fido2ChallengePurpose.Registration, options.ToJson());

            return options;
        }

        public async Task<Fido2Credential> CompleteAssertionAsync(
            UserWeb2 user,
            AuthenticatorAssertionRawResponse assertion,
            CancellationToken? cancellationToken = null)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(assertion);

            var originalOptions = await ConsumeChallengeAsync(user.Id, Fido2ChallengePurpose.Assertion, AssertionOptions.FromJson);

            var credential = user.FindFido2Credential(assertion.RawId) ??
                throw new InvalidOperationException("FIDO2 credential not registered for this user.");

            var ct = cancellationToken ?? CancellationToken.None;
            var result = await fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = assertion,
                OriginalOptions = originalOptions,
                StoredPublicKey = credential.PublicKey,
                StoredSignatureCounter = credential.SignatureCounter,
                IsUserHandleOwnerOfCredentialIdCallback = (args, token) =>
                    Task.FromResult(user.FindFido2Credential(args.CredentialId) is not null)
            }, ct);

            user.RecordFido2CredentialUsage(credential.CredentialId, result.SignCount);

            return credential;
        }

        public async Task<Fido2Credential> CompleteRegistrationAsync(
            UserWeb2 user,
            AuthenticatorAttestationRawResponse attestation,
            string nickname,
            CancellationToken? cancellationToken = null)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(attestation);

            var originalOptions = await ConsumeChallengeAsync(user.Id, Fido2ChallengePurpose.Registration, CredentialCreateOptions.FromJson);

            var ct = cancellationToken ?? CancellationToken.None;
            var result = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = attestation,
                OriginalOptions = originalOptions,
                IsCredentialIdUniqueToUserCallback = (args, token) =>
                    Task.FromResult(user.FindFido2Credential(args.CredentialId) is null)
            }, ct);

            var transports = result.Transports?.Select(t => t.ToString()).ToList() ?? [];
            var credential = new Fido2Credential(
                credentialId: result.Id,
                publicKey: result.PublicKey,
                signatureCounter: result.SignCount,
                nickname: nickname,
                transports: transports);

            if (!user.AddFido2Credential(credential))
                throw new InvalidOperationException("FIDO2 credential already registered for this user.");

            return credential;
        }

        // Helpers.
        private static AuthenticatorTransport[] ParseTransports(IEnumerable<string> transports)
        {
            //Stored as enum names (see CompleteRegistrationAsync); skip any value we can't map back
            //so an unknown/future transport never breaks the assertion options.
            var parsed = new List<AuthenticatorTransport>();
            foreach (var t in transports)
                if (Enum.TryParse<AuthenticatorTransport>(t, ignoreCase: true, out var value))
                    parsed.Add(value);
            return [.. parsed];
        }

        private async Task<T> ConsumeChallengeAsync<T>(string userId, Fido2ChallengePurpose purpose, Func<string, T> fromJson)
        {
            var challenge = await ssoDbContext.Fido2Challenges.TryFindOneAsync(c => c.User.Id == userId && c.Purpose == purpose) ??
                throw new InvalidOperationException($"No pending FIDO2 challenge of type {purpose} for user.");

            if (challenge.ExpiresAt < DateTime.UtcNow)
            {
                await ssoDbContext.Fido2Challenges.DeleteAsync(challenge);
                throw new InvalidOperationException("FIDO2 challenge expired.");
            }

            var options = fromJson(challenge.OptionsJson);
            await ssoDbContext.Fido2Challenges.DeleteAsync(challenge);
            return options;
        }

        private async Task PersistChallengeAsync(UserWeb2 user, Fido2ChallengePurpose purpose, string optionsJson)
        {
            //Replace any previous pending challenge for this (user, purpose) to keep at most one in flight.
            var existing = await ssoDbContext.Fido2Challenges.TryFindOneAsync(c => c.User.Id == user.Id && c.Purpose == purpose);
            if (existing is not null)
                await ssoDbContext.Fido2Challenges.DeleteAsync(existing);

            var challenge = new Fido2Challenge(user, purpose, optionsJson);
            await ssoDbContext.Fido2Challenges.CreateAsync(challenge);
        }
    }
}
