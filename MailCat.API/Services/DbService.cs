using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCat.API.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace MailCat.API.Services
{
    public class DbService
    {
        private IConfiguration Config { get; }

        public DbService(IConfiguration config)
        {
            Config = config;
        }

        public IMongoDatabase BuildDatabaseClient(DatabaseSettings dbSettings)
        {
            Console.WriteLine($"Configured with these Settings: {Environment.NewLine}" +
                              $"{JToken.FromObject(dbSettings).ToString()}");
            MongoClient client;
            if (string.IsNullOrWhiteSpace(Config["MONGO_URL"]))
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
                client = new MongoClient(clientSettings);

            }
            else
            {
                client = new MongoClient(Config["MONGO_URL"]);
            }

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
