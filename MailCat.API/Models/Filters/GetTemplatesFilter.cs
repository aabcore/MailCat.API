using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailCat.API.Models.Filters
{
    public class GetTemplatesFilter:IFilter
    {
        public string NameContains { get; set; }
        public string DescriptionContains { get; set; }
        public DateTimeOffset? DateBefore { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
    }
}
