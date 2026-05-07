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

namespace Etherna.SSOServer.Domain.Models.ClientAppAgg
{
    public class ClientSecret : ModelBase
    {
        // Constructors.
        public ClientSecret(
            string value,
            string? description,
            DateTime? expiration)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));

            Created = DateTime.UtcNow;
            Description = description;
            Expiration = expiration;
            Value = value;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        protected ClientSecret() { }
#pragma warning restore CS8618

        // Properties.
        public virtual DateTime Created { get; protected set; }
        public virtual string? Description { get; protected set; }
        public virtual DateTime? Expiration { get; protected set; }
        public virtual bool IsExpired => Expiration.HasValue && DateTime.UtcNow > Expiration.Value;
        public virtual string Value { get; protected set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (obj is not ClientSecret other)
                return false;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();
    }
}
