using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSugar.Filters
{
    public class FilteredQuery<T>
    {
        public IQueryable<T> Query { get; set; }
        public int RecordCount { get; set; }

        public FilteredResult<T> Resolve()
        {
            return new FilteredResult<T>() { RecordCount = RecordCount, Value = Query.ToList() };
        }
    }
}
