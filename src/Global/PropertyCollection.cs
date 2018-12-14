using EFCoreSugar.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Global
{
    internal static class PropertyCollection
    {
        private const BindingFlags _BindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
        internal static Dictionary<Type, IEnumerable<FilterProperty>> TypeProperties { get; } = new Dictionary<Type, IEnumerable<FilterProperty>>();

        internal static IEnumerable<FilterProperty> RegisterFilterProperties(Type type)
        {
            var props = type.GetProperties(_BindingFlags).Where(p => !Attribute.IsDefined(p, typeof(FilterIgnoreAttribute)));
            List<FilterProperty> propsList = new List<FilterProperty>();

            foreach (var prop in props)
            {
                propsList.Add(new FilterProperty(prop, prop.GetCustomAttribute<FilterPropertyAttribute>()));
            }

            if (!TypeProperties.ContainsKey(type))
            {
                TypeProperties.Add(type, propsList);
            }
            else
            {
                TypeProperties[type] = propsList;
            }

            return propsList;
        }
    }
}
