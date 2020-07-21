using System;

namespace MailCat.API.Models.Filters
{
    public class GetMailFilter:IFilter
    {
        public DateTimeOffset? Before { get; set; }
        public DateTimeOffset? After { get; set; }
        public string ToEmail { get; set; }
        public string FromEmail { get; set; }
        public int Limit { get; set; }
        public int Skip { get; set; }
    }
}