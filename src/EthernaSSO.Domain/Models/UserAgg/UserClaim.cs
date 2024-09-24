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

using System;

namespace Etherna.SSOServer.Domain.Models.UserAgg
{
    public class UserClaim : ModelBase
    {
        // Constructors.
        public UserClaim(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type can't be empty", nameof(type));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value can't be empty", nameof(value));

            Type = type;
            Value = value;
        }
        protected UserClaim() { }

        // Properties.
        public virtual string Type { get; protected set; } = default!;
        public virtual string Value { get; protected set; } = default!;

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not UserClaim claim2) return false;
            return Type == claim2.Type && Value == claim2.Value;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode(StringComparison.InvariantCulture) ^
                Value.GetHashCode(StringComparison.InvariantCulture);
        }
    }
}
