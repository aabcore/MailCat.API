using System;
using MongoDB.Bson;

namespace MailCat.API.Models.ApiDtos
{
    public class TemplateOutDto
    {
        public ObjectId Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}