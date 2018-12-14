using EFCoreSugar.Filters;
using EFCoreSugar.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreSugar
{
    public static class EFCoreSugarGlobal
    {
        public static void BuildFilters()
        {
            var types = GetAllTypesInAssemblies(typeof(Filter));
            foreach (var type in types)
            {
                PropertyCollection.RegisterFilterProperties(type);
                //TODO: prebuild some of the expressions here also?
            }
        }

        internal static IEnumerable<Type> GetAllTypesInAssemblies(Type type)
        {
            var domain = AppDomain.CurrentDomain;

            var assemblies = domain.GetAssemblies();

            List<Type> types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                types.AddRange(assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t)));
            }

            return types;
        }
    }
}
