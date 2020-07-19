using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MailCat.API.Models.ApiDtos
{
    public class MailOutDto
    {
        public ObjectId Id { get; set; }
        public string From { get; set; }
        public IEnumerable<string> ToRecipients { get; set; }
        public IEnumerable<string> CcRecipients { get; set; }
        public IEnumerable<string> BccRecipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
