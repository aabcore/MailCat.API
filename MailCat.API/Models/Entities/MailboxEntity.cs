using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MailCat.API.Models.Entities
{
    //public class MailboxEntity
    //{
    //    [BsonId]
    //    public long Id { get; set; }
    //    public string Email { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //}

    public class MailEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string From { get; set; }
        public IEnumerable<string> ToRecipients { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<string> CcRecipients { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<string> BccRecipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset Date { get; set; }
    }

    public class TemplateEntity
    {
        [BsonId]
        public long Id { get; set; }
        public string SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; }
    }
}
