﻿// Copyright 2021-present Etherna Sa
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

using Duende.IdentityServer.Services;
using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.EventHandlers
{
    internal sealed class OnUserLoginSuccessThenNotifyIdentityServerHandler : EventHandlerBase<UserLoginSuccessEvent>
    {
        // Fields.
        private readonly IEventService identityServerEventService;

        // Constructor.
        public OnUserLoginSuccessThenNotifyIdentityServerHandler(
            IEventService identityServerEventService)
        {
            this.identityServerEventService = identityServerEventService;
        }

        // Methods.
        public override async Task HandleAsync(UserLoginSuccessEvent @event)
        {
            await identityServerEventService.RaiseAsync(
                @event.Provider is null ?
                new Duende.IdentityServer.Events.UserLoginSuccessEvent(
                    @event.User.Username,
                    @event.User.Id,
                    @event.User.Username,
                    clientId: @event.ClientId) :
                new Duende.IdentityServer.Events.UserLoginSuccessEvent(
                    @event.Provider,
                    @event.ProviderUserId,
                    @event.User.Id,
                    @event.User.Username,
                    clientId: @event.ClientId));
        }
    }
}
