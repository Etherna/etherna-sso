﻿//   Copyright 2021-present Etherna Sagl
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

namespace Etherna.SSOServer.Services.Views.Emails
{
    public class AlphaPassLetterModel
    {
        public const string Title = "Etherna is here";

        public AlphaPassLetterModel(
            string invitationCode,
            string invitationUrl,
            string userName)
        {
            InvitationCode = invitationCode;
            InvitationUrl = invitationUrl;
            UserName = userName;
        }

        public string InvitationCode { get; }
        public string InvitationUrl { get; }
        public string UserName { get; }
    }
}
