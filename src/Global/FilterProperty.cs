using EFCoreSugar.Filters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Global
{
    internal class FilterProperty
    {
        internal PropertyInfo Property { get; set; }
        internal FilterPropertyAttribute PropertyAttribute { get; set; }
        internal FilterOperation Operation { get; set; }

        public FilterProperty(PropertyInfo property, FilterPropertyAttribute propertyAttribute, FilterOperation operation)
        {
            Property = property;
            PropertyAttribute = propertyAttribute;
            Operation = operation;
        }
    }
}
