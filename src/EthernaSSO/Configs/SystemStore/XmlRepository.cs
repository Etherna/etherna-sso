﻿//   Copyright 2021-present Etherna Sagl
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

using Etherna.MongODM.Core.Options;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Etherna.SSOServer.Configs.SystemStore
{
    public class XmlRepository : IXmlRepository
    {
        // Fields.
        private readonly IMongoCollection<BsonDocument> collection;

        // Constructors.
        public XmlRepository(DbContextOptions options, string name)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            // Initialize MongoDB driver.
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DbName);
            collection = database.GetCollection<BsonDocument>(name);
        }

        // Methods.
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return collection.AsQueryable().ToList().Select(bsonDoc =>
            {
                //remove unnecessary document id added by mongodb
                bsonDoc.Remove("_id");

                var jsonStr = bsonDoc.ToJson();
                var xDocument = JsonConvert.DeserializeXNode(jsonStr)!;
                if (xDocument.Root is null)
                    throw new InvalidOperationException();
                return xDocument.Root;
            }).ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            //remove all comments. Json doesn't support it, but Json.NET serialize them anyway
            element.DescendantNodes().Where(x => x.NodeType == XmlNodeType.Comment).Remove();

            var jsonStr = JsonConvert.SerializeXNode(element);
            var bsonDoc = BsonDocument.Parse(jsonStr);
            collection.InsertOne(bsonDoc);
        }
    }
}
