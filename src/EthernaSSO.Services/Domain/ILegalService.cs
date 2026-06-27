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
using System.Collections.Generic;

namespace Etherna.SSOServer.Services.Domain
{
    /// <summary>
    /// A legal document the user must accept to register, resolved to its currently published
    /// version and public URL.
    /// </summary>
    public sealed record LegalDocument(LegalDocumentType Type, string Version, string Url);

    public interface ILegalService
    {
        /// <summary>
        /// The legal documents that must be accepted to create an account, in display order.
        /// </summary>
        IReadOnlyList<LegalDocument> RequiredDocuments { get; }

        /// <summary>
        /// Build an acceptance record for every required document, stamped at the current instant
        /// with the version currently published. The returned records are the proof of consent to
        /// persist together with the new user.
        /// </summary>
        IEnumerable<LegalAcceptance> BuildAcceptancesForRequiredDocuments();
    }
}
