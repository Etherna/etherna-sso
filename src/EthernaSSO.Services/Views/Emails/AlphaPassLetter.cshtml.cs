﻿// Copyright 2021-present Etherna SA
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

namespace Etherna.SSOServer.Services.Views.Emails
{
    public class AlphaPassLetterModel
    {
        public const string Title = "Your Alpha Pass for Etherna";

        public AlphaPassLetterModel(
            string invitationCode,
            string invitationUrl)
        {
            InvitationCode = invitationCode;
            InvitationUrl = invitationUrl;
        }

        public string InvitationCode { get; }
        public string InvitationUrl { get; }
    }
}
