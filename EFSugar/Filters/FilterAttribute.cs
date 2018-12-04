using System;
using System.Collections.Generic;
using System.Text;

namespace EFSugar.Filters
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FilterProperty : Attribute
    {
        public int GroupNumber { get; set; }
        public string PropertyName { get; set; }
        public FilterOperation Operation { get; set; }
        public FilterTest Test { get; set; }
        public FilterProperty(int groupNumber, FilterOperation operation = FilterOperation.And)
        {
            GroupNumber = groupNumber;
        }

        public FilterProperty(int groupNumber, string propertyName, FilterOperation operation = FilterOperation.And)
        {
            GroupNumber = groupNumber;
            PropertyName = propertyName;
            Operation = operation;
        }

        public FilterProperty(string propertyName, FilterOperation operation = FilterOperation.And)
        {
            PropertyName = propertyName;
        }
    }
}
