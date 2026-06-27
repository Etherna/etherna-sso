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
using Etherna.SSOServer.Services.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.SSOServer.Services.Domain
{
    internal sealed class LegalService(IOptions<LegalOptions> options) : ILegalService
    {
        // Fields.
        private readonly LegalOptions options = options.Value;

        // Properties.
        public IReadOnlyList<LegalDocument> RequiredDocuments =>
        [
            new(LegalDocumentType.TermsOfService, options.TermsOfService.Version, options.TermsOfService.Url),
            new(LegalDocumentType.PrivacyPolicy, options.PrivacyPolicy.Version, options.PrivacyPolicy.Url)
        ];

        // Methods.
        public IEnumerable<LegalAcceptance> BuildAcceptancesForRequiredDocuments()
        {
            // A single timestamp for the whole consent action, captured once at acceptance time.
            var acceptanceDateTime = DateTime.UtcNow;
            return RequiredDocuments
                .Select(d => new LegalAcceptance(d.Type, d.Version, acceptanceDateTime))
                .ToList();
        }
    }
}
