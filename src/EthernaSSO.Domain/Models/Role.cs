using Etherna.MongODM.Core.Attributes;
using System;
using System.Globalization;

namespace Etherna.SSOServer.Domain.Models
{
    public class Role : EntityModelBase<string>
    {
        // Constructors and dispose.
        public Role(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
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
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (Name != name)
            {
                Name = name;
                NormalizedName = NormalizeName(name);
            }
        }

        // Public static helpers.
        public static string NormalizeName(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            name = name.ToUpper(CultureInfo.InvariantCulture); //to upper case

            return name;
        }
    }
}
