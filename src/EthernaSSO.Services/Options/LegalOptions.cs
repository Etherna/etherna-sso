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

namespace Etherna.SSOServer.Services.Options
{
    public sealed class LegalOptions
    {
        // Types.
        public sealed class DocumentOptions
        {
            public string Url { get; set; } = null!;

            /// <summary>
            /// Identifier of the currently published revision of the document (e.g. an ISO date or a
            /// semantic version). It is stored verbatim in each acceptance record, so it must change
            /// whenever the document text materially changes.
            /// </summary>
            public string Version { get; set; } = null!;
        }

        // Properties.
        public DocumentOptions PrivacyPolicy { get; set; } = new();
        public DocumentOptions TermsOfService { get; set; } = new();
    }
}
