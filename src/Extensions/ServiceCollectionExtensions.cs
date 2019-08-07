using EFCoreSugar.Global;
using EFCoreSugar.Repository;
using EFCoreSugar.Repository.Interfaces;
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
            return RegisterType(collection, typeof(IBaseRepositoryGroup), ServiceLifetime.Transient);
        }

        public static IServiceCollection RegisterBaseRepositories(this IServiceCollection collection, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return RegisterType(collection, typeof(IBaseDbRepository), lifetime);
        }

        private static IServiceCollection RegisterType(IServiceCollection collection, Type baseType, ServiceLifetime lifetime)
        {
            var types = EFCoreSugarGlobal.GetAllTypesInAssemblies(baseType);

            foreach (var type in types)
            {
                collection.AddTransient(type);
                var interfaceType = type.GetInterface($"I{type.Name}", true);
                if (interfaceType != null)
                {
                    collection.Add(new ServiceDescriptor(interfaceType, type, lifetime));
                }
            }
            return collection;
        }
    }
}
