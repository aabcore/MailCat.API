using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCat.API.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace MailCat.API.Services
{
    public class DbService
    {
        public IMongoDatabase BuildDatabaseClient(DatabaseSettings dbSettings)
        {
            Console.WriteLine($"Configured with these Settings: {Environment.NewLine}" +
                              $"{JToken.FromObject(dbSettings).ToString()}");
            var credential = MongoCredential.CreateCredential("admin",
                dbSettings.DatabaseUserName,
                dbSettings.DatabasePassword);
            var clientSettings = new MongoClientSettings()
            {
                Credential = credential,
                Server = MongoServerAddress.Parse(dbSettings.ConnectionString),
                AllowInsecureTls = true
            };
            var client = new MongoClient(clientSettings);

            // DB Configuration
            var pack = new ConventionPack()
            {
                new EnumRepresentationConvention(BsonType.String)
            };
            ConventionRegistry.Register("EnumStringConvention", pack, t => true);

            return client.GetDatabase(dbSettings.DatabaseName);
        }

        public FindOneAndUpdateOptions<T> GetEntityAfterUpdateOption<T>() => new FindOneAndUpdateOptions<T>()
        {
            ReturnDocument = ReturnDocument.After
        };
    }
}
