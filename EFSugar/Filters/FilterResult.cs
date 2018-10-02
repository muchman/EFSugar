using System;
using System.Collections.Generic;
using System.Text;

namespace EFSugar.Filters
{
    public class FilterResult<T>
    {

        public T Query { get; set; }
        public int RecordCount { get; set; }
    }
}
