using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MailCat.API.Models.Entities
{
    public class TemplateEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}