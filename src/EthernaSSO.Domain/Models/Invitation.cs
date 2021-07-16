using System;

namespace Etherna.SSOServer.Domain.Models
{
    public class Invitation : EntityModelBase<string>
    {
        // Constructors.
        public Invitation(TimeSpan? duration, UserBase emitter, bool isSingleUse)
        {
            Code = Guid.NewGuid().ToString();
            Emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
            if (duration is not null)
                EndLife = DateTime.Now + duration;
            IsSingleUse = isSingleUse;
        }
        protected Invitation() { }

        // Properties.
        public virtual string Code { get; protected set; } = default!;
        public virtual UserBase Emitter { get; protected set; } = default!;
        public virtual DateTime? EndLife { get; protected set; }
        public virtual bool IsAlive => EndLife is null || DateTime.Now < EndLife;
        public virtual bool IsSingleUse { get; protected set; }
    }
}
