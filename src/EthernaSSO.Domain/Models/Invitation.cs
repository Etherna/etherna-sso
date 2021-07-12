using System;

namespace Etherna.SSOServer.Domain.Models
{
    public class Invitation : EntityModelBase<string>
    {
        // Constructors.
        public Invitation(UserBase owner)
        {
            Code = Guid.NewGuid();
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        protected Invitation() { }

        // Properties.
        public virtual Guid Code { get; protected set; }
        public virtual UserBase Owner { get; protected set; } = default!;
    }
}
