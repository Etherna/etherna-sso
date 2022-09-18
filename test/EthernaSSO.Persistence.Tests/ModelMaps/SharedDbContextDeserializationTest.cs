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
using System.Linq;
using Xunit;

namespace Etherna.SSOServer.Persistence.ModelMaps
{
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
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

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
