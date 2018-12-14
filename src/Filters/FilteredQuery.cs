using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreSugar.Filters
{
    public class FilteredQuery<T> where T : class
    {
        public IQueryable<T> Query { get; set; }
        private PagingFilter PageFilter { get; }

        internal FilteredQuery(IQueryable<T> query, PagingFilter pageFilter)
        {
            Query = query;
            PageFilter = pageFilter;
        }

        public FilteredResult<T> Resolve()
        {
            var pagingResult = PageFilter.ApplyFilter(Query);
            return new FilteredResult<T>() { RecordCount = pagingResult.RecordCount, Value = pagingResult.Query.ToList() };
        }

    }
}
