using EFCoreSugar.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Filters
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class FilterOperationAttribute : Attribute
    {
        public FilterOperation Operation { get; }
        public FuzzyMatchMode FuzzyMode { get; }

        public FilterOperationAttribute(FilterOperation operation)
        {
            Operation = operation;
            FuzzyMode = FuzzyMatchMode.Contains;
        }

        [Obsolete("This method is deprecated, please use FuzzyMatchAttribute for FuzzyMatchMode")]
        public FilterOperationAttribute(FilterOperation operation, FuzzyMatchMode mode)
        {
            Operation = operation;
            FuzzyMode = mode;
        }

        [Obsolete("This method is deprecated, please use FuzzyMatchAttribute for FuzzyMatchMode")]
        public FilterOperationAttribute(FuzzyMatchMode mode)
        {
            FuzzyMode = mode;
        }
    }
}
