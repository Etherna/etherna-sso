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
