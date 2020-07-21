using System;
using System.Collections.Generic;
using MailCat.API.Controllers;
using MongoDB.Bson;

namespace MailCat.API.Models.ApiDtos
{
    public class TemplateRevisionOutDto
    {
        public ObjectId Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public ObjectId TemplateReference { get; set; }
        public long RevisionNumber { get; set; }
        public string SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; }
        public string DefaultFrom { get; set; }
        public IEnumerable<string> DefaultToRecipients { get; set; }
        public IEnumerable<string> DefaultCcRecipients { get; set; }
        public IEnumerable<string> DefaultBccRecipients { get; set; }
    }
}