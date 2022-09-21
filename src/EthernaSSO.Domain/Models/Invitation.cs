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
