﻿using EFCoreSugar.Filters;
using System;
using System.Collections.Generic;
using System.Text;

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
    }

    public class UserFilter : Filter
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public int Age { get; set; }
    }
}
