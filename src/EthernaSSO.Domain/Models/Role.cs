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
