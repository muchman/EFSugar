using EFSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EFSugar
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
            var domain = AppDomain.CurrentDomain;

            var assemblies = domain.GetAssemblies();

            List<Type> types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                types.AddRange(assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t)));
            }

            foreach (var type in types)
            {
                collection.AddTransient(type);
            }
            return collection;
        }
    }
}
