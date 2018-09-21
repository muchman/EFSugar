using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSugar.Filters
{
    public class PagingFilter<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public FilterResult<IQueryable<T>> ApplyFilter(IQueryable<T> query)
        {
            var result = new FilterResult<IQueryable<T>>();
            result.RecordCount = query.Count();
            result.Result = query.Skip((PageNumber > 0 ? PageNumber - 1 : 0) * PageSize).Take(PageSize);
            return result;
        }
    }
}
