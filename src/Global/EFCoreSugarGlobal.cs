using EFCoreSugar.Filters;
using EFCoreSugar.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                EFCoreSugarPropertyCollection.RegisterFilterProperties(type);
                //TODO: prebuild some of the expressions here also?
            }
        }

        internal static IEnumerable<Type> GetAllTypesInAssemblies(Type type)
        {
            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToList();

            List<Type> types = new List<Type>();

            referencedPaths.ForEach(path =>
            {
                var loadedAssembly = Assembly.LoadFrom(path);
                types.AddRange(loadedAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t)));
            });

            return types;
        }
    }
}
