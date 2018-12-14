using EFCoreSugar;
using EFCoreSugar.Filters;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Tests.FakeDatabase;
using Tests.FakeEntities;
using Tests.FilterTestGoup;
using Tests.RepoTests;
using Xunit;

namespace Tests.FilterTestGoup
{
    public class FilterTests : BaseTest
    {
        [Fact]
        public void FilterExtensionTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            SeedData();

            var filter = new OrderFilter() { NestedOrderTypeId = 1, UId = 1 };

            //should be 3
            var orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(3);

        }

        [Fact]
        public void NestedPropertyTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            SeedData();

            var filter = new OrderFilter() { UId = 1, PName = "Shoes" };

            //should be 2
            var orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(2);

        }

        [Fact]
        public void PagingTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            SeedData();

            var filter = new OrderFilter() { OrderTypeId = 1, PageNumber = 1, PageSize = 5 };

            //should be 5, count 25
            var orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(25);
            orders.Value.Count().Should().Be(5);
            orders.Value.First().UserId.Should().Be(1);

            //advance the page and we should have same record count but starting on the 6th record
            filter.PageNumber = 2;
            filter.PageSize = 5;
            orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(25);
            orders.Value.Count().Should().Be(5);
            orders.Value.First().Id.Should().Be(7);
        }

        [Fact]
        public void OrderByTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            SeedData();

            var filter = new OrderFilter() { OrderTypeId = 1};
            filter.SetOrderBy(x => x.UId); //ascending

            var orders = repo.Filter<Order>(filter);
            orders.Value.First().UserId.Should().Be(1);

            filter.SetOrderBy(x => x.UId, OrderByDirection.Descending); //descending

            orders = repo.Filter<Order>(filter);
            orders.Value.First().UserId.Should().Be(10);

            //default field will be Id since its called Id, but also has a key value
            filter = new OrderFilter() { OrderTypeId = 1 };
            orders = repo.Filter<Order>(filter);
            orders.Value.First().Id.Should().Be(1);

            filter.OrderByDirection = OrderByDirection.Descending;
            orders = repo.Filter<Order>(filter);
            orders.Value.First().Id.Should().Be(28);

        }
    }
}
