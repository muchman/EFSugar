using EFSugar.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.FilterTestGoup
{
    public class TestFilter : Filter
    { 
        public string Name { get; set; }
    }

    internal class TestFilter2: Filter
    {
        public int One { get; set; }
        public int Two { get; set; }
        public int Three { get; set; }
        public int Four { get; set; }
    }
}
