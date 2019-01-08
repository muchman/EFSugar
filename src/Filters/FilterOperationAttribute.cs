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
        public FuzzySearchMode FuzzyMode { get; }

        public FilterOperationAttribute(FilterOperation operation, FuzzySearchMode mode = FuzzySearchMode.Contains)
        {
            Operation = operation;
            FuzzyMode = mode;
        }

        public FilterOperationAttribute(FuzzySearchMode mode)
        {
            FuzzyMode = mode;
        }
    }
}
