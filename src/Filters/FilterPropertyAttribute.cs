using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Filters
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FilterPropertyAttribute : Attribute
    {
        public string PropertyName { get; set; }
        public FilterTest Test { get; set; }

        public FilterPropertyAttribute(string propertyName, FilterTest test = FilterTest.Equal)
        {
            PropertyName = propertyName;
            Test = test;
        }

        public FilterPropertyAttribute(FilterTest test)
        {
            Test = test;
        }

    }
}
