using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCat.API.Models.Filters;

namespace MailCat.API.Models.ApiDtos
{
    public class FilteredDtoOut<TFilter, TData> where TFilter : IFilter
    {
        public FilteredDtoOut(TFilter filterUsed, IEnumerable<TData> data)
        {
            FilterUsed = filterUsed;
            Data = data;
        }
        public TFilter FilterUsed { get; set; }
        public IEnumerable<TData> Data { get; set; }
    }
}