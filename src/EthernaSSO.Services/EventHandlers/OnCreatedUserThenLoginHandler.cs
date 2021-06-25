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

using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    class OnCreatedUserThenLoginHandler : EventHandlerBase<EntityCreatedEvent<User>>
    {
        // Fields.
        private readonly SignInManager<User> signInManager;

        // Constructors.
        public OnCreatedUserThenLoginHandler(
            SignInManager<User> signInManager)
        {
            this.signInManager = signInManager;
        }

        // Methods.
        public override Task HandleAsync(EntityCreatedEvent<User> @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));

            return signInManager.SignInAsync(@event.Entity, false);
        }
    }
}
