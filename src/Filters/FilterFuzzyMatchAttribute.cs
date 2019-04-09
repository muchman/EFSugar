using EFCoreSugar.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Filters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class FilterFuzzyMatchAttribute : Attribute
    {
        public FuzzyMatchMode FuzzyMatchMode { get; }

        public FilterFuzzyMatchAttribute(FuzzyMatchMode mode)
        {
            FuzzyMatchMode = mode;
        }
    }
}
