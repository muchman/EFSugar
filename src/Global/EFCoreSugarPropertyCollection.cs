using EFCoreSugar.Enumerations;
using EFCoreSugar.Filters;
using EFCoreSugar.Repository;
using EFCoreSugar.Repository.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Global
{
    internal static class EFCoreSugarPropertyCollection
    {
        private const BindingFlags _BindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        internal static ConcurrentDictionary<Type, FilterCache> FilterTypeProperties { get; } = new ConcurrentDictionary<Type, FilterCache>();
        internal static ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> BaseDbRepositoryGroupTypeProperties { get; } = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        internal static ConcurrentDictionary<Type, string> DefaultOrderByTypeProperties { get; } = new ConcurrentDictionary<Type, string>();

        internal static IEnumerable<PropertyInfo> RegisterBaseDbRepositoryGroupProperties(Type type)
        {
            var props = type.GetProperties(_BindingFlags).Where(pi => typeof(IBaseRepositoryGroup).IsAssignableFrom(pi.PropertyType));

            BaseDbRepositoryGroupTypeProperties.AddOrUpdate(type, props, (key, old) => props);

            return props;
        }

        internal static IEnumerable<FilterProperty> RegisterFilterProperties(Type type)
        {
            var props = type.GetProperties(_BindingFlags | BindingFlags.NonPublic).Where(p => !Attribute.IsDefined(p, typeof(FilterIgnoreAttribute)));
            List<FilterProperty> propsList = new List<FilterProperty>();

            var baseFilterOperation = type.GetCustomAttribute<FilterOperationAttribute>();
            var baseFuzzyMatchMode = type.GetCustomAttribute<FilterFuzzyMatchAttribute>();

            foreach (var prop in props)
            {
                //TODO: Remove the BaseFilterOperation.FuzzyMode from here in v2, it is obsolete
                propsList.Add(new FilterProperty(prop, prop.GetCustomAttribute<FilterPropertyAttribute>(), 
                    prop.GetCustomAttribute<FilterOperationAttribute>()?.Operation ?? baseFilterOperation?.Operation ?? FilterOperation.And,
                    prop.GetCustomAttribute<FilterFuzzyMatchAttribute>()?.FuzzyMatchMode ?? baseFuzzyMatchMode?.FuzzyMatchMode ?? baseFilterOperation?.FuzzyMode ?? FuzzyMatchMode.Contains));
            }
      
            var cache = new FilterCache(propsList, baseFilterOperation);

            FilterTypeProperties.AddOrUpdate(type, cache, (key, old) => cache);

            return propsList;
        }


        internal static string RegisterDefaultOrderByProperty(Type type)
        {
            string defaultName = null;
            var props = type.GetProperties();

            foreach(var prop in props)
            {
                if(Attribute.IsDefined(prop, typeof(KeyAttribute)) || prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    defaultName = prop.Name;
                }
            }

            defaultName = defaultName ?? props.First().Name;
            DefaultOrderByTypeProperties.AddOrUpdate(type, defaultName, (key, old) => defaultName);

            return defaultName;
        }

        internal static string GetDefaultPropertyName(Type type)
        {
            if (!DefaultOrderByTypeProperties.TryGetValue(type, out var defaultPropName))
            {
                defaultPropName = RegisterDefaultOrderByProperty(type);
            }

            return defaultPropName;
        }
    }
}
