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

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    /// <summary>
    /// A legal document a user can be required to accept. Persisted by name (not by ordinal),
    /// so members can be reordered without altering the meaning of stored acceptance records.
    /// </summary>
    public enum LegalDocumentType
    {
        PrivacyPolicy,
        TermsOfService
    }
}
