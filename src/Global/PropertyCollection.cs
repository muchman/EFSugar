using EFCoreSugar.Filters;
using System;
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
        internal static Dictionary<Type, IEnumerable<FilterProperty>> FilterTypeProperties { get; } = new Dictionary<Type, IEnumerable<FilterProperty>>();

        internal static Dictionary<Type, OrderByProperties> OrderByTypeProperties { get; } = new Dictionary<Type, OrderByProperties>();

        internal static IEnumerable<FilterProperty> RegisterFilterProperties(Type type)
        {
            var props = type.GetProperties(_BindingFlags).Where(p => !Attribute.IsDefined(p, typeof(FilterIgnoreAttribute)));
            List<FilterProperty> propsList = new List<FilterProperty>();

            foreach (var prop in props)
            {
                propsList.Add(new FilterProperty(prop, prop.GetCustomAttribute<FilterPropertyAttribute>()));
            }

            if (!FilterTypeProperties.ContainsKey(type))
            {
                FilterTypeProperties.Add(type, propsList);
            }
            else
            {
                FilterTypeProperties[type] = propsList;
            }

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
