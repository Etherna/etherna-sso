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
