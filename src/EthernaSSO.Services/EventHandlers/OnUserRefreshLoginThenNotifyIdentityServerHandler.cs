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

using Duende.IdentityServer.Services;
using Etherna.DomainEvents;
using Etherna.SSOServer.Domain.Events;
using System.Threading.Tasks;
using UserLoginSuccessEvent = Duende.IdentityServer.Events.UserLoginSuccessEvent;

namespace Etherna.SSOServer.Services.EventHandlers
{
    internal sealed class OnUserRefreshLoginThenNotifyIdentityServerHandler(
        IEventService identityServerEventService)
        : EventHandlerBase<UserRefreshLoginEvent>
    {
        public override Task HandleAsync(UserRefreshLoginEvent @event) =>
            identityServerEventService.RaiseAsync(
                new UserLoginSuccessEvent(
                    @event.User.Username,
                    @event.User.Id,
                    @event.User.Username,
                    clientId: @event.ClientId));
    }
}