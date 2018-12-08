using EFSugar;
using EFSugar.Filters;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Tests.FakeDatabase;
using Tests.FakeEntities;
using Tests.FilterTestGoup;
using Xunit;

namespace Tests.RepoTests
{
    public class RepositoryTests
    {

        private IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddTransient<FakeRepo>();
            services.AddTransient<TestDbContext>();
            return services.RegisterRepositoryGroups();

        }

        [Fact]
        public void RepoInstantiator()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var servicesProvider = services.BuildServiceProvider();
            servicesProvider.GetService<FakeRepo>();
            servicesProvider.GetService<FakeRepo>();
        }



        [Fact]
        public void FilterExtensionTest()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var servicesProvider = services.BuildServiceProvider();
            var repo = servicesProvider.GetService<FakeRepo>();
            

            ((TestDbContext)repo.DBContext).SeedData();

            var filter = new OrderFilter() { NestedOrderTypeId = 1, UId = 1 };

            //should be 2
            var orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(3);
                       
        }

        [Fact]
        public void NestedPropertyTest()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var servicesProvider = services.BuildServiceProvider();
            var repo = servicesProvider.GetService<FakeRepo>();


            ((TestDbContext)repo.DBContext).SeedData();

            var filter = new OrderFilter() { UId = 1, PName = "Shoes" };

            //should be 2
            var orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(2);

        }

        [Fact]
        public void PagingTest()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var servicesProvider = services.BuildServiceProvider();
            var repo = servicesProvider.GetService<FakeRepo>();


            ((TestDbContext)repo.DBContext).SeedData();

            var filter = new OrderFilter() { OrderTypeId = 1, PageNumber = 1, PageSize = 5 };

            //should be 5, count 25
            var orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(25);
            orders.Value.Count().Should().Be(5);
            orders.Value.First().UserId.Should().Be(1);

            //advance the page and we should have same record count but starting on the 6th record
            filter = new OrderFilter() { OrderTypeId = 1, PageNumber = 2, PageSize = 5 };
            orders = repo.Filter<Order>(filter);
            orders.RecordCount.Should().Be(25);
            orders.Value.Count().Should().Be(5);
            orders.Value.First().Id.Should().Be(7);
        }

    }
}
