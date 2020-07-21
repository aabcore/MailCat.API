using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailCat.API.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUserName { get; set; }
        public string DatabasePassword { get; set; }
        public string MailCollectionName { get; set; }
        public string TemplateCollectionName { get; set; }
        public string TemplateRevisionCollectionName { get; set; }
    }
}
