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

using Etherna.MongODM.Core.Attributes;
using System;
using System.Globalization;

namespace Etherna.SSOServer.Domain.Models
{
    public class Role : EntityModelBase<string>
    {
        // Consts.
        public const string AdministratorName = "Administrator";

        // Constructors and dispose.
        public Role(string name)
        {
            SetName(name);
        }
        protected Role() { }

        // Properties.
        public virtual string Name { get; protected set; } = default!;
        public virtual string NormalizedName { get; protected set; } = default!;

        // Methods.

        [PropertyAlterer(nameof(Name))]
        [PropertyAlterer(nameof(NormalizedName))]
        public virtual void SetName(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            if (Name != name)
            {
                Name = name;
                NormalizedName = NormalizeName(name);
            }
        }

        // Public static helpers.
        public static string NormalizeName(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            name = name.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return name;
        }
    }
}
