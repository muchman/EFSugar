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
        internal FilterPropertyAttribute Attribute { get; set; }

        internal FilterProperty(PropertyInfo property, FilterPropertyAttribute attribute)
        {
            Property = property;
            Attribute = attribute;
        }
    }
}
