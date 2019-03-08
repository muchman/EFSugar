using EFCoreSugar.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Filters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class FuzzyMatchAttribute : Attribute
    {
        public FuzzyMatchMode FuzzyMatchMode { get; }

        public FuzzyMatchAttribute(FuzzyMatchMode mode)
        {
            FuzzyMatchMode = mode;
        }
    }
}
