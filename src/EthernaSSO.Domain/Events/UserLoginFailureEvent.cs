//   Copyright 2021-present Etherna Sagl
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

using Etherna.DomainEvents;
using System;

namespace Etherna.SSOServer.Domain.Events
{
    public class UserLoginFailureEvent : IDomainEvent
    {
        public UserLoginFailureEvent(string identifier, string error, string? clientId)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException($"'{nameof(error)}' cannot be null or whitespace.", nameof(error));

            ClientId = clientId;
            Error = error;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public string? ClientId { get; }
        public string Error { get; }
        public string Identifier { get; }
    }
}
