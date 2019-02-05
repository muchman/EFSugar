using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeEntities;

namespace Tests.FakeDatabase.FakeEntities
{
    public class Part
    {
        public int Id { get; set; }
        public string PartName { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
