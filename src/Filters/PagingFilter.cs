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

        public (IQueryable<T> Query, int RecordCount) ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            var count = query.Count();

            //this really only works if you have a pagesize, otherwise how many do you skip or take?
            if (PageSize > 0)
            {
                return (query.Skip((PageNumber > 0 ? PageNumber - 1 : 0) * PageSize).Take(PageSize), count);
            }
            else
            {
                return (query, count);
            }
        }
    }
}
