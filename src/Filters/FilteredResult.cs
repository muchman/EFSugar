using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Filters
{
    public class FilteredResult<T>
    {
        public IEnumerable<T> Value { get; set; }
        public int RecordCount { get; set; }
    }
}
