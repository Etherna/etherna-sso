using System;

namespace Etherna.SSOServer.Domain.Models
{
    public class Invitation : EntityModelBase<string>
    {
        // Constructors.
        public Invitation(UserBase owner)
        {
            Code = Guid.NewGuid().ToString();
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        protected Invitation() { }

        // Properties.
        public virtual string Code { get; protected set; } = default!;
        public virtual UserBase Owner { get; protected set; } = default!;
    }
}
