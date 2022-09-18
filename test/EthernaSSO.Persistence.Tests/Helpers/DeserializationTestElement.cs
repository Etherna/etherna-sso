using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Moq;
using System;

namespace Etherna.SSOServer.Persistence.Helpers
{
    public class DeserializationTestElement<TModel, TDbContext>
        where TDbContext : IDbContext
    {
        public DeserializationTestElement(string sourceDocument, TModel expectedModel) :
            this(sourceDocument, expectedModel, (_, _) => { })
        { }

        public DeserializationTestElement(
            string sourceDocument,
            TModel expectedModel,
            Action<Mock<IMongoDatabase>, TDbContext> setupAction)
        {
            SourceDocument = sourceDocument;
            ExpectedModel = expectedModel;
            SetupAction = setupAction;
        }

        public string SourceDocument { get; }
        public TModel ExpectedModel { get; }
        public Action<Mock<IMongoDatabase>, TDbContext> SetupAction { get; }
    }
}
