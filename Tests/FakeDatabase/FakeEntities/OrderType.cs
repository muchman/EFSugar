using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tests.FakeEntities
{
    public class OrderType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
