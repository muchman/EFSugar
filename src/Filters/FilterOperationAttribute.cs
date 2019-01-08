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

        public FilterOperationAttribute(FilterOperation operation, FuzzyMatchMode mode = FuzzyMatchMode.Contains)
        {
            Operation = operation;
            FuzzyMode = mode;
        }

        public FilterOperationAttribute(FuzzyMatchMode mode)
        {
            FuzzyMode = mode;
        }
    }
}
