using EFSugar.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.FilterTestGoup
{
    public class TestFilter : Filter
    {
        [FilterProperty("Name")]
        public string NameNotName { get; set; }
        [FilterProperty(1,Test = EFSugar.Enumerations.FilterTest.LessThan)]
        public int Balance { get; set; }
    }

    public class TestFilter2: Filter
    {

        public int? One { get; set; }
        public int? Two { get; set; }
        public int? Three { get; set; }
        public int? Four { get; set; }
    }
}
