using EFSugar;
using Microsoft.Extensions.DependencyInjection;
using Tests.FakeDatabase;
using Xunit;

namespace Tests.RepoTests
{
    public class RepositoryTests
    {
        [Fact]
        public void RepoInstantiator()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            var servicesProvider = services.BuildServiceProvider();
            servicesProvider.GetService<FakeRepo>();
            servicesProvider.GetService<FakeRepo>();
        }

        private IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddTransient<FakeRepo>();
            services.AddTransient<TestDbContext>();
            return services.RegisterRepositoryGroups();
            
        }
    }
}
