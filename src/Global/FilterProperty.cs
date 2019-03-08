using EFCoreSugar.Enumerations;
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
        public string PropertyName { get; set; }
        public string[] SplitPropertyName { get; set; }
        internal FilterOperation Operation { get; set; }
        internal FilterTest Test { get; set; }
        internal FuzzyMatchMode FuzzyMatchMode {get; set;}

        public FilterProperty(PropertyInfo property, FilterPropertyAttribute propertyAttribute, FilterOperation operation, FuzzyMatchMode fuzzyMatchMode)
        {
            Property = property;
            Operation = operation;
            PropertyName = propertyAttribute?.PropertyName ?? Property.Name;
            SplitPropertyName = PropertyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Test = propertyAttribute?.Test ?? FilterTest.Equal;
            FuzzyMatchMode = fuzzyMatchMode;
        }
    }
}
