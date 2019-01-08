using EFCoreSugar.Filters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreSugar.Global
{
    internal class FilterCache
    {
        internal IEnumerable<FilterProperty> FilterProperties { get; set; }
        internal FilterOperationAttribute OperationAttribute { get; set; }

        public FilterCache(IEnumerable<FilterProperty> filterProperties, FilterOperationAttribute operationAttribute)
        {
            FilterProperties = filterProperties;
            OperationAttribute = operationAttribute;
        }
    }
}
