using EFCoreSugar;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeDatabase;
using Xunit;

namespace Tests.OtherTests
{
    public class ServiceProviderTests
    {
        [Fact]
        public void RegisterRepositoriesTest()
        {
            var services = new ServiceCollection();
            services.RegisterBaseRepositories();
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<FakeRepo>().Should().NotBeNull();
        }
    }
}
