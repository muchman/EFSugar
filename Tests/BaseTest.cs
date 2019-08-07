using EFCoreSugar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.FakeDatabase;
using Tests.FakeEntities;
using Tests.RepoTests;

namespace Tests
{
    public class BaseTest
    {
        public IServiceProvider ServiceProvider { get; set; }
        private static int _initCounter = 0;
        public BaseTest()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            ConfigureDbContext(services);
            ServiceProvider = services.BuildServiceProvider();

        }

        private IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.RegisterBaseRepositories();
            services.RegisterRepositoryGroups();
            services.AddScoped<TestDbContext>();
            return services;
        }

        protected virtual void ConfigureDbContext(IServiceCollection services)
        {
            var name = GetType().Name;

            services.AddDbContext<TestDbContext>(opts =>
                opts.UseInMemoryDatabase(name + "-" + _initCounter++));
        }

        protected virtual void SeedData()
        {
            var context = ServiceProvider.GetService<TestDbContext>();

            context.Add(new User() { Id = 1, FirstName = "Bob", LastName = "Turtle", Age = 35, DOB = DateTime.Parse("1/1/1983") });
            context.Add(new User() { Id = 2, FirstName = "Jon", LastName = "Conway", Age = 47, DOB = DateTime.Parse("1/1/1982") });
            context.Add(new User() { Id = 3, FirstName = "Ted", LastName = "Peabody", Age = 12, DOB = DateTime.Parse("1/1/1981") });
            context.Add(new User() { Id = 4, FirstName = "Marissa", LastName = "Snail", Age = 67, DOB = DateTime.Parse("1/1/1951") });
            context.Add(new User() { Id = 5, FirstName = "Henry", LastName = "Merkle", Age = 45, DOB = DateTime.Parse("1/1/1978") });
            context.Add(new User() { Id = 6, FirstName = "Lisa", LastName = "Harrison", Age = 18, DOB = DateTime.Parse("1/1/1977") });
            context.Add(new User() { Id = 7, FirstName = "Joanna", LastName = "Parrot", Age = 23, DOB = DateTime.Parse("1/1/1976") });
            context.Add(new User() { Id = 8, FirstName = "Mike", LastName = "Turtle", Age = 25, DOB = DateTime.Parse("1/1/1975") });
            context.Add(new User() { Id = 9, FirstName = "Roger", LastName = "Snakeface", Age = 67, DOB = DateTime.Parse("1/1/1951") });
            context.Add(new User() { Id = 10, FirstName = "Bob", LastName = "Slowman", Age = 67, DOB = DateTime.Parse("1/1/1951") });

            context.Add(new OrderType() { Id = 2, Name = "Return" });

            context.Add(new Order() { Id = 1, UserId = 1, OrderTypeId = 1, ProductName = "Shoes", Value = 100 });
            context.Add(new Order() { Id = 2, UserId = 1, OrderTypeId = 1, ProductName = "Coat", Value = 10 });
            context.Add(new Order() { Id = 3, UserId = 2, OrderTypeId = 1, ProductName = "Coat", Value = 100 });
            context.Add(new Order() { Id = 4, UserId = 2, OrderTypeId = 2, ProductName = "Coat", Value = 10 });
            context.Add(new Order() { Id = 5, UserId = 2, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 6, UserId = 3, OrderTypeId = 1, ProductName = "Shirt", Value = 10 });
            context.Add(new Order() { Id = 7, UserId = 4, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 8, UserId = 4, OrderTypeId = 1, ProductName = "Pants", Value = 10 });
            context.Add(new Order() { Id = 9, UserId = 4, OrderTypeId = 2, ProductName = "Pants", Value = 10 });
            context.Add(new Order() { Id = 10, UserId = 4, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 11, UserId = 5, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 12, UserId = 6, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 13, UserId = 7, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 14, UserId = 8, OrderTypeId = 1, ProductName = "Hat", Value = 10 });
            context.Add(new Order() { Id = 15, UserId = 9, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 16, UserId = 10, OrderTypeId = 1, ProductName = "Sunglasses", Value = 10 });
            context.Add(new Order() { Id = 17, UserId = 10, OrderTypeId = 1, ProductName = "Shirt", Value = 10 });
            context.Add(new Order() { Id = 18, UserId = 1, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 19, UserId = 2, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 20, UserId = 5, OrderTypeId = 1, ProductName = "Shoes", Value = 100 });
            context.Add(new Order() { Id = 21, UserId = 8, OrderTypeId = 1, ProductName = "Pants", Value = 10 });
            context.Add(new Order() { Id = 22, UserId = 7, OrderTypeId = 1, ProductName = "Shoes", Value = 100 });
            context.Add(new Order() { Id = 23, UserId = 7, OrderTypeId = 2, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 24, UserId = 9, OrderTypeId = 1, ProductName = "Pants", Value = 10 });
            context.Add(new Order() { Id = 25, UserId = 4, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 26, UserId = 6, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.Add(new Order() { Id = 27, UserId = 8, OrderTypeId = 1, ProductName = "Hat", Value = 10 });
            context.Add(new Order() { Id = 28, UserId = 10, OrderTypeId = 1, ProductName = "Shoes", Value = 10 });
            context.SaveChanges();
        }

        protected void AddData<T>(List<T> data) where T : class
        {
            var context = ServiceProvider.GetService<TestDbContext>();
            context.Add(data);
        }

        protected IEnumerable<T> ReadData<T>() where T : class
        {
            var context = ServiceProvider.GetService<TestDbContext>();
            return context.Set<T>().ToList();
        }
    }
}
