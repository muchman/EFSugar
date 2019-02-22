using EFCoreSugar.Enumerations;
using EFCoreSugar.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeEntities;

namespace Tests.FilterTestGoup
{
    public class OrderFilter : Filter
    {
        public int? Id { get; set; }
        [FilterProperty("UserId")]
        public int? UId { get; set; }
        [FilterProperty("ProductName")]
        public string PName { get; set; }
        public int? Value { get; set; }
        public int? OrderTypeId { get; set; }
        [FilterProperty("OrderType.Id")]
        public int? NestedOrderTypeId { get; set; }
        [FilterProperty(FilterTest.GreaterThanEqualTo)]
        public DateTimeOffset? OrderDateTime { get; set; }
    }

    public class UserFilter : Filter
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [FilterProperty(FilterTest.GreaterThanEqualTo)]
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
    }

    [FilterOperation(FilterOperation.Or, FuzzyMatchMode.Contains)]
    public class UserFilterOr : Filter
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
    }

    public class UserOrderNavigationPropFilter : Filter
    {
        [FilterProperty("Orders.ProductName")]
        public string ProductName { get; set; }

        [FilterProperty("Orders.Parts.PartName")]
        public string PartName { get; set; }
    }

    public class CollectionFilter : Filter
    {
        public int? Id { get; set; }
        [FilterProperty("Status")]
        public List<OrderStatus> StatusList { get; set; }
    }
}


