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
    public class Invitation : EntityModelBase<string>
    {
        // Constructors.
        public Invitation(TimeSpan? duration, UserBase? emitter, bool isFromAdmin, bool isSingleUse)
        {
            Code = Guid.NewGuid().ToString();
            Emitter = emitter;
            if (duration is not null)
                EndLife = DateTime.UtcNow + duration;
            IsFromAdmin = isFromAdmin;
            IsSingleUse = isSingleUse;
        }
        protected Invitation() { }

        // Properties.
        public virtual string Code { get; protected set; } = default!;
        public virtual UserBase? Emitter { get; protected set; } = default!;
        public virtual DateTime? EndLife { get; protected set; }
        public virtual bool IsAlive => EndLife is null || DateTime.UtcNow < EndLife;
        public virtual bool IsFromAdmin { get; protected set; }
        public virtual bool IsSingleUse { get; protected set; }
    }
}
