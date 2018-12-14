using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreSugar.Filters
{
    public class PagingFilter
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public FilteredQuery<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            var result = new FilteredQuery<T>();
            result.RecordCount = query.Count();

            //this really only works if you have a pagesize, otherwise how many do you skip or take?
            if (PageSize > 0)
            {
                result.Query = query.Skip((PageNumber > 0 ? PageNumber - 1 : 0) * PageSize).Take(PageSize);
            }
            else
            {
                result.Query = query;
            }
            return result;
        }
    }
}
