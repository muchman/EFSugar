using System;
using System.Collections.Generic;
using System.Text;

namespace EFSugar.Filters
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FilterProperty : Attribute
    {
        public string PropertyName { get; set; }
        public FilterTest Test { get; set; }

        public FilterProperty(string propertyName, FilterTest test = FilterTest.Equal)
        {
            PropertyName = propertyName;
            Test = test;
        }

        public FilterProperty(FilterTest test)
        {
            Test = test;
        }

    }
}
