using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Tests.FakeDatabase.FakeEntities;

namespace Tests.FakeEntities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ProductName { get; set; }
        public int Value { get; set; }
        public int OrderTypeId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("OrderTypeId")]
        public OrderType OrderType { get; set; }

        public ICollection<Part> Parts { get; set; }
        public DateTimeOffset OrderDateTime { get; set; }
    }
}
