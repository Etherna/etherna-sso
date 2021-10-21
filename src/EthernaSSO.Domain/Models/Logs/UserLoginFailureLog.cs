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

namespace Etherna.SSOServer.Domain.Models.Logs
{
    public class UserLoginFailureLog : LogBase
    {
        // Constructors.
        public UserLoginFailureLog(string error, string identifier)
        {
            ErrorMessage = error;
            Identifier = identifier;
        }
        protected UserLoginFailureLog() { }

        // Properties.
        public virtual string ErrorMessage { get; protected set; } = default!;
        public virtual string Identifier { get; protected set; } = default!;
    }
}
