using EFCoreSugar.Global;
using EFCoreSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EFCoreSugar
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterRepositoryGroups(this IServiceCollection collection)
        {
            return RegisterType(collection, typeof(IRepositoryGroup));
        }

        public static IServiceCollection RegisterBaseRepositories(this IServiceCollection collection)
        {
            return RegisterType(collection, typeof(IBaseDbRepository));
        }

        private static IServiceCollection RegisterType(IServiceCollection collection, Type interfaceType)
        {
            var types = EFCoreSugarGlobal.GetAllTypesInAssemblies(interfaceType);

            foreach (var type in types)
            {
                collection.AddTransient(type);
            }
            return collection;
        }
    }
}
