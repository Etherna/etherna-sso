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

namespace Etherna.SSOServer.Domain.Models
{
    public enum Fido2ChallengePurpose
    {
        Assertion,
        Registration
    }

    public class Fido2Challenge : EntityModelBase<string>
    {
        // Consts.
        public static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(5);

        // Constructors.
        public Fido2Challenge(
            UserBase user,
            Fido2ChallengePurpose purpose,
            string optionsJson,
            TimeSpan? lifetime = null)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(optionsJson);

            ExpiresAt = DateTime.UtcNow + (lifetime ?? DefaultLifetime);
            OptionsJson = optionsJson;
            Purpose = purpose;
            User = user;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Fido2Challenge() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        // Properties.
        public virtual DateTime ExpiresAt { get; protected set; }
        public virtual string OptionsJson { get; protected set; }
        public virtual Fido2ChallengePurpose Purpose { get; protected set; }
        public virtual UserBase User { get; protected set; }
    }
}
