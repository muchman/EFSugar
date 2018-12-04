using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFSugar.Filters
{

    //I need a way to self reference the filter.  I cant say this T inside of the filter and I cant expose the properties another way that I know of
    public static class FilterExtensions
    {

        public static void SetOrderBy<T>(this T filter, Expression<Func<T, object>> expression, OrderByDirection direction) where T : Filter
        {
            filter.OrderByDirection = direction;

            var unaryExpression = (UnaryExpression)expression.Body;
            var memberExpression = ((MemberExpression)unaryExpression.Operand);

            var filterProperty = memberExpression.Member.GetCustomAttribute<FilterProperty>();

            if (filterProperty != null && !String.IsNullOrWhiteSpace(filterProperty.PropertyName))
            {
                filter.PropertyName = filterProperty.PropertyName;
            }
            else
            {
                filter.PropertyName = memberExpression.Member.Name;
            }
        }
    }
}
