using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MailCat.API.Models.Entities
{
    public class TemplateRevisionEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        public ObjectId TemplateReference { get; set; }
        public long RevisionNumber { get; set; }
        public string SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; }

        [BsonIgnoreIfNull]
        public string DefaultFrom { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<string> DefaultToRecipients { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<string> DefaultCcRecipients { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<string> DefaultBccRecipients { get; set; }
    }
}