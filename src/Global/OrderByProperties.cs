using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Global
{
    internal class OrderByProperties
    {
        internal Dictionary<string, PropertyInfo> Properties { get; set; } = new Dictionary<string, PropertyInfo>();
        internal PropertyInfo DefaultOrderBy { get; set; }
    }
}
