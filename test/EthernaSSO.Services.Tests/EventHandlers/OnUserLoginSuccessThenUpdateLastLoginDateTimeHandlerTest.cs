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

using Etherna.SSOServer.Domain;
using Etherna.SSOServer.Domain.Events;
using Etherna.SSOServer.Domain.Models;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SwarmSdk.Models;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.SSOServer.Services.EventHandlers
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test method naming convention.")]
    public class OnUserLoginSuccessThenUpdateLastLoginDateTimeHandlerTest
    {
        // Consts.
        private const string UserId = "6a317235d33d390f7d1e350e";

        // Tests.
        [Fact]
        public async Task HandleAsync_ReloadsTheUserOnTheHandlerDbContextAndUpdatesItsLastLogin()
        {
            // Arrange.
            //the event carries the instance of the scope that raised it: the handler works on the one of its own scope
            var eventUserMock = new Mock<UserBase>();
            eventUserMock.Setup(u => u.Id).Returns(UserId);
            var scopeUser = new UserWeb3(
                "scopeuser",
                invitedBy: null,
                invitedByAdmin: false,
                new UserSharedInfo(new EthAddress("0x123f681646d4a755815f9cb19e1acc8565a0c2ac")));
            var ssoDbContextMock = new Mock<ISsoDbContext>();
            ssoDbContextMock.Setup(c => c.Users.FindOneAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(scopeUser);
            var handler = new OnUserLoginSuccessThenUpdateLastLoginDateTimeHandler(ssoDbContextMock.Object);
            var loginStart = DateTime.UtcNow;

            // Action.
            await handler.HandleAsync(new UserLoginSuccessEvent(eventUserMock.Object));

            // Assert.
            Assert.InRange(scopeUser.LastLoginDateTime, loginStart, DateTime.UtcNow);
            ssoDbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
