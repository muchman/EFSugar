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

        internal static ConcurrentDictionary<Type, IEnumerable<FilterProperty>> FilterTypeProperties { get; } = new ConcurrentDictionary<Type, IEnumerable<FilterProperty>>();
        internal static ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> BaseDbRepositoryGroupTypeProperties { get; } = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        internal static ConcurrentDictionary<Type, OrderByProperties> OrderByTypeProperties { get; } = new ConcurrentDictionary<Type, OrderByProperties>();

        internal static IEnumerable<PropertyInfo> RegisterBaseDbRepositoryGroupProperties(Type type)
        {
            var props = type.GetProperties(_BindingFlags).Where(pi => typeof(IBaseRepositoryGroup).IsAssignableFrom(pi.PropertyType));

            BaseDbRepositoryGroupTypeProperties.AddOrUpdate(type, props, (key, old) => props);

            return props;
        }

        internal static IEnumerable<FilterProperty> RegisterFilterProperties(Type type)
        {
            var props = type.GetProperties(_BindingFlags).Where(p => !Attribute.IsDefined(p, typeof(FilterIgnoreAttribute)));
            List<FilterProperty> propsList = new List<FilterProperty>();

            foreach (var prop in props)
            {
                propsList.Add(new FilterProperty(prop, prop.GetCustomAttribute<FilterPropertyAttribute>()));
            }

            FilterTypeProperties.AddOrUpdate(type, propsList, (key, old) => propsList);

            return propsList;
        }


        internal static OrderByProperties RegisterOrderByProperties(Type type)
        {
            var orderByProperties = new OrderByProperties();
            var props = type.GetProperties();

            foreach(var prop in props)
            {
                orderByProperties.Properties.Add(prop.Name, prop);
                if(Attribute.IsDefined(prop, typeof(KeyAttribute)) || prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    orderByProperties.DefaultOrderBy = prop;
                }
            }

            orderByProperties.DefaultOrderBy = orderByProperties.DefaultOrderBy ?? props.First();

            return orderByProperties;
        }
    }
}
