﻿using Etherna.MongODM;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Etherna.SSOServer.DataProtectionStore
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
