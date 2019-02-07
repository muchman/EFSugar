using EFCoreSugar;
using EFCoreSugar.Filters;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Tests.FakeDatabase;
using Tests.FakeDatabase.FakeEntities;
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

        [Fact]
        public void FilterComposeTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            SeedData();

            var filter1 = new OrderFilter() { OrderTypeId = 1, PageNumber = 1, PageSize = 1 };
            var filter2 = new OrderFilter() { PName = "Shoes", PageNumber = 1, PageSize = 5 };//paging should be ignored, we only allow the first filter to decide paging for now
            filter1.SetOrderBy(x => x.UId, OrderByDirection.Descending); //descending

            var orders = repo.GetQueryable<Order>();
            var firstquery = orders.Filter(filter1);
            var secondquery = firstquery.Filter(filter2);
            var actualorders = secondquery.Resolve();

            actualorders.RecordCount.Should().Be(15); //15 pre-paged
            actualorders.Value.Count().Should().Be(1);
            actualorders.Value.First().UserId.Should().Be(10);
        }

        [Fact]
        public void FilterComposeTestOrderBy()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new Order() { Id = 1, UserId = 5, OrderTypeId = 1, ProductName = "B", Value = 100 });
            context.Add(new Order() { Id = 2, UserId = 6, OrderTypeId = 1, ProductName = "B", Value = 100 });
            context.Add(new Order() { Id = 3, UserId = 3, OrderTypeId = 1, ProductName = "C", Value = 100 });
            context.Add(new Order() { Id = 4, UserId = 6, OrderTypeId = 1, ProductName = "A", Value = 100 });
            context.Add(new Order() { Id = 5, UserId = 5, OrderTypeId = 1, ProductName = "A", Value = 100 });
            context.Add(new Order() { Id = 6, UserId = 3, OrderTypeId = 1, ProductName = "A", Value = 100 });
            context.SaveChanges();

            var filter1 = new OrderFilter() { Value = 100, OrderByPropertyName = "UId"};
            filter1.SetOrderBy(f => f.UId);

            var filter2 = new OrderFilter() { OrderByPropertyName = "PName"};//paging should be ignored, we only allow the first filter to decide paging for now

            var orders = repo.GetQueryable<Order>();
            var firstquery = orders.Filter(filter1);
            var secondquery = firstquery.Filter(filter2);
            var actualorders = secondquery.Resolve();
        }

        [Fact]
        public void GlobalLoaderTest()
        {
            EFCoreSugarGlobal.BuildFilters();
        }

        [Fact]
        public void FuzzySearchTermTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new Order() { Id = 1, UserId = 5, OrderTypeId = 1, ProductName = "new", Value = 100 });
            context.Add(new Order() { Id = 2, UserId = 6, OrderTypeId = 1, ProductName = "kindaoldyeah", Value = 100 });
            context.Add(new Order() { Id = 3, UserId = 3, OrderTypeId = 1, ProductName = "notold", Value = 100 });
            context.Add(new Order() { Id = 4, UserId = 6, OrderTypeId = 1, ProductName = "oldandnew", Value = 100 });
            context.Add(new Order() { Id = 5, UserId = 5, OrderTypeId = 1, ProductName = "win", Value = 100 });
            context.Add(new Order() { Id = 6, UserId = 3, OrderTypeId = 1, ProductName = "cookie", Value = 100 });
            context.SaveChanges();

            var filter1 = new OrderFilter() { FuzzyMatchTerm = "old"};


            var orders = repo.GetQueryable<Order>();
            var filtered = orders.Filter(filter1).Resolve();
            filtered.Value.Count().Should().Be(3);
            filtered.Value.Count(x => x.ProductName == "kindaoldyeah").Should().Be(1);
            filtered.Value.Count(x => x.ProductName == "notold").Should().Be(1);
            filtered.Value.Count(x => x.ProductName == "oldandnew").Should().Be(1);
        }

        [Fact]
        public void FuzzySearchTermTestWithId()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new Order() { Id = 1, UserId = 5, OrderTypeId = 1, ProductName = "new", Value = 100 });
            context.Add(new Order() { Id = 2, UserId = 6, OrderTypeId = 1, ProductName = "kindaoldyeah", Value = 100 });
            context.Add(new Order() { Id = 3, UserId = 3, OrderTypeId = 1, ProductName = "notold", Value = 100 });
            context.Add(new Order() { Id = 4, UserId = 6, OrderTypeId = 1, ProductName = "oldandnew", Value = 100 });
            context.Add(new Order() { Id = 5, UserId = 5, OrderTypeId = 1, ProductName = "win", Value = 100 });
            context.Add(new Order() { Id = 6, UserId = 3, OrderTypeId = 1, ProductName = "cookie", Value = 100 });
            context.SaveChanges();

            var filter1 = new OrderFilter() { UId = 3, FuzzyMatchTerm = "old" };


            var orders = repo.GetQueryable<Order>();
            var filtered = orders.Filter(filter1).Resolve();
            filtered.Value.Count().Should().Be(1);
            filtered.Value.Count(x => x.ProductName == "notold").Should().Be(1);
        }

        [Fact]
        public void FilterOperationTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new User() { Id = 1, FirstName = "Bob", LastName = "Turtle", Age = 35, DOB = DateTime.Parse("1/1/1983") });
            context.Add(new User() { Id = 2, FirstName = "Jon", LastName = "Conway", Age = 47, DOB = DateTime.Parse("1/1/1982") });
            context.Add(new User() { Id = 3, FirstName = "Ted", LastName = "Peabody", Age = 47, DOB = DateTime.Parse("1/1/1981") });
            context.Add(new User() { Id = 4, FirstName = "Marissa", LastName = "Snail", Age = 67, DOB = DateTime.Parse("1/1/1951") });
            context.SaveChanges();

            var filter1 = new UserFilterOr() { FirstName = "Bob", Age = 47 };


            var orders = repo.GetQueryable<User>();
            var filtered = orders.Filter(filter1).Resolve();
            filtered.Value.Count().Should().Be(3);
            filtered.Value.Count(x => x.FirstName == "Bob").Should().Be(1);
            filtered.Value.Count(x => x.Age == 47).Should().Be(2);
        }

        [Fact]
        public void NullableTest()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new User() { Id = 1, FirstName = "Bob", LastName = "Turtle", Age = 35, DOB = DateTime.Parse("1/1/1983") });
            context.SaveChanges();

            var filter1 = new UserFilter() { DOB = DateTime.Parse("1/1/1983") };

            var orders = repo.GetQueryable<User>();
            var filtered = orders.Filter(filter1).Resolve();
            filtered.Value.Count().Should().Be(1);
        }


        [Fact]
        public void NavigationCollectionPropertyFilter()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new User() { Id = 1, FirstName = "Bob", LastName = "Turtle", Age = 35, DOB = DateTime.Parse("1/1/1983") });
            context.Add(new User() { Id = 2, FirstName = "Don", LastName = "Bear", Age = 20, DOB = DateTime.Parse("1/1/1981") });
            context.Add(new Order() { Id = 1, UserId = 1, ProductName = "Thing" });
            context.Add(new Order() { Id = 2, UserId = 2, ProductName = "Thing2" });
            context.Add(new Order() { Id = 3, UserId = 2, ProductName = "Thing3" });
            context.Add(new Part() { Id = 1, OrderId = 2, PartName = "Part2" });
            context.Add(new Part() { Id = 2, OrderId = 3, PartName = "Part3" });
            context.SaveChanges();

            //var result = context.Users.Where(u => u.Orders.Any(o => o.Parts.Any(p => p.PartName == "Part2"))).ToList();
            var filter = new UserOrderNavigationPropFilter() { PartName = "Part3" };

            var orders = repo.GetQueryable<User>().Include(u => u.Orders).ThenInclude(o => o.Parts);
            var filtered = orders.Filter(filter).Resolve();
            filtered.Value.Count().Should().Be(1);
        }

        [Fact]
        public void NavigationCollectionPropertyFuzzy()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new User() { Id = 1, FirstName = "Bob", LastName = "Turtle", Age = 35, DOB = DateTime.Parse("1/1/1983") });
            context.Add(new User() { Id = 2, FirstName = "Don", LastName = "Bear", Age = 20, DOB = DateTime.Parse("1/1/1981") });
            context.Add(new Order() { Id = 1, UserId = 1, ProductName = "Thing" });
            context.Add(new Order() { Id = 2, UserId = 2, ProductName = "Thing2" });
            context.Add(new Order() { Id = 3, UserId = 2, ProductName = "Thing3" });
            context.Add(new Part() { Id = 1, OrderId = 1, PartName = "Part1" });
            context.Add(new Part() { Id = 2, OrderId = 2, PartName = "Part2" });
            context.Add(new Part() { Id = 3, OrderId = 3, PartName = "Part3" });
            context.SaveChanges();

            var filter = new UserOrderNavigationPropFilter() { FuzzyMatchTerm = "Part" };

            var orders = repo.GetQueryable<User>().Include(u => u.Orders).ThenInclude(o => o.Parts);
            var filtered = orders.Filter(filter).Resolve();
            filtered.Value.Count().Should().Be(2);
        }

        [Fact]
        public void NavigationCollectionPropertyOrderBy()
        {
            var repo = ServiceProvider.GetService<FakeRepo>();
            //special data setup
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(new User() { Id = 1, FirstName = "Bob", LastName = "Turtle", Age = 35, DOB = DateTime.Parse("1/1/1983") });
            context.Add(new User() { Id = 2, FirstName = "Don", LastName = "Bear", Age = 20, DOB = DateTime.Parse("1/1/1981") });
            context.Add(new Order() { Id = 1, UserId = 1, ProductName = "Thing" });
            context.Add(new Order() { Id = 2, UserId = 2, ProductName = "Thing2" });
            context.Add(new Order() { Id = 3, UserId = 2, ProductName = "Thing3" });
            context.Add(new Part() { Id = 1, OrderId = 1, PartName = "Part1" });
            context.Add(new Part() { Id = 2, OrderId = 2, PartName = "Part2" });
            context.Add(new Part() { Id = 3, OrderId = 3, PartName = "Part3" });
            context.SaveChanges();


            //var result = context.Users.OrderByDescending(u => u.Orders.FirstOrDefault().Parts.FirstOrDefault().PartName);

            var filter = new UserOrderNavigationPropFilter() { OrderByPropertyName = "PartName", OrderByDirection = OrderByDirection.Descending };


            var orders = repo.GetQueryable<User>().Include(u => u.Orders).ThenInclude(o => o.Parts);
            var filtered = orders.Filter(filter).Resolve();
            filtered.Value.Count().Should().Be(2);
            filtered.Value.First().Orders.First().Parts.First().PartName.Should().Be("Part2");
        }
    }
}
