﻿// Copyright 2021-present Etherna SA
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

using Etherna.DomainEvents;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.MongODM.Core.Utility;
using Etherna.SSOServer.Domain.Models.UserAgg;
using Etherna.SSOServer.Persistence.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
    public class SharedDbContextDeserializationTest
    {
        // Fields.
        private readonly SharedDbContext dbContext;
        private readonly Mock<IEventDispatcher> eventDispatcherMock = new();
        private readonly Mock<IMongoDatabase> mongoDatabaseMock = new();

        // Constructor.
        public SharedDbContextDeserializationTest()
        {
            // Setup dbContext.
            dbContext = new SharedDbContext(eventDispatcherMock.Object);

            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);
        }

        // Data.
        public static IEnumerable<object[]> UserSharedInfoDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<UserSharedInfo, SharedDbContext>>();

                // "6d0d2ee1-6aa3-42ea-9833-ac592bfc6613" - from sso v0.3.0
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""61cdffb4fa7c4052d258adcb""), 
                            ""_m"" : ""6d0d2ee1-6aa3-42ea-9833-ac592bfc6613"", 
                            ""CreationDateTime"" : ISODate(""2021-12-30T18:51:32.706+0000""), 
                            ""EtherAddress"" : ""0x410211F4824A8f7EDf174B32AB215924557b4437"", 
                            ""EtherPreviousAddresses"" : [
                                ""0x6401ceD81d2e864f214A93823647F5baBF819123""
                            ], 
                            ""LockoutEnabled"" : true, 
                            ""LockoutEnd"" : ISODate(""2022-12-30T18:51:32.706+0000""),
                        }";

                    var expectedSharedInfoMock = new Mock<UserSharedInfo>();
                    expectedSharedInfoMock.Setup(i => i.Id).Returns("61cdffb4fa7c4052d258adcb");
                    expectedSharedInfoMock.Setup(i => i.CreationDateTime).Returns(new DateTime(2021, 12, 30, 18, 51, 32, 706));
                    expectedSharedInfoMock.Setup(i => i.EtherAddress).Returns("0x410211F4824A8f7EDf174B32AB215924557b4437");
                    expectedSharedInfoMock.Setup(i => i.EtherPreviousAddresses).Returns(new[] { "0x6401ceD81d2e864f214A93823647F5baBF819123" });
                    expectedSharedInfoMock.Setup(i => i.LockoutEnabled).Returns(true);
                    expectedSharedInfoMock.Setup(i => i.LockoutEnd).Returns(new DateTimeOffset(2022, 12, 30, 18, 51, 32, 706, TimeSpan.Zero));

                    tests.Add(new DeserializationTestElement<UserSharedInfo, SharedDbContext>(sourceDocument, expectedSharedInfoMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(UserSharedInfoDeserializationTests))]
        public void UserSharedInfoDeserialization(DeserializationTestElement<UserSharedInfo, SharedDbContext> testElement)
        {
            ArgumentNullException.ThrowIfNull(testElement, nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<UserSharedInfo>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.EtherAddress, result.EtherAddress);
            Assert.Equal(testElement.ExpectedModel.EtherPreviousAddresses, result.EtherPreviousAddresses);
            Assert.Equal(testElement.ExpectedModel.LockoutEnabled, result.LockoutEnabled);
            Assert.Equal(testElement.ExpectedModel.LockoutEnd, result.LockoutEnd);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.EtherAddress);
            Assert.NotNull(result.EtherPreviousAddresses);
        }
    }
}
