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
            return RegisterType(collection, typeof(IBaseRepositoryGroup));
        }

        public static IServiceCollection RegisterBaseRepositories(this IServiceCollection collection)
        {
            return RegisterType(collection, typeof(IBaseDbRepository));
        }

        private static IServiceCollection RegisterType(IServiceCollection collection, Type baseType)
        {
            var types = EFCoreSugarGlobal.GetAllTypesInAssemblies(baseType);

            foreach (var type in types)
            {
                collection.AddTransient(type);
                var interfaceType = type.GetInterface($"I{type.Name}", true);
                if (interfaceType != null)
                {
                    collection.AddTransient(interfaceType, type);
                }
            }
            return collection;
        }
    }
}
