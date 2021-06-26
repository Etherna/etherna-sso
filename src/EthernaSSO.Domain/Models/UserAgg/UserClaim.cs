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
