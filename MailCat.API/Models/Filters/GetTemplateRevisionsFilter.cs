using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailCat.API.Models.Filters
{
    public class GetTemplateRevisionsFilter: IFilter
    {
        public DateTimeOffset? DateBefore { get; set; }
        public int Limit { get; set; }
        public int Skip { get; set; }
    }
}
