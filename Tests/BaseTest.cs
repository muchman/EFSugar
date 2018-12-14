using EFCoreSugar;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.FakeDatabase;
using Tests.RepoTests;

namespace Tests
{
    public class BaseTest
    {
        public IServiceProvider ServiceProvider { get; set; }
        public BaseTest()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            ServiceProvider = services.BuildServiceProvider();

        }

        private IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddTransient<FakeRepo>();
            services.AddTransient<TestDbContext>();
            return services.RegisterRepositoryGroups();
        }

        protected virtual void SeedData()
        {
            var context = ServiceProvider.GetService<TestDbContext>();
            context.SeedData();
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
