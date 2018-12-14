using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Filters
{
    public class OrderByFilter
    {
        private const BindingFlags _BindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
        public OrderByDirection OrderByDirection { get; set; }
        public string PropertyName { get; set; }
        public IQueryable<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            PropertyInfo prop = null;
            var type = typeof(T);

            if(String.IsNullOrWhiteSpace(PropertyName))
            {
                var props = type.GetProperties();
                prop = props.First(p => p.Name.ToLower() == "id" || Attribute.IsDefined(p, typeof(KeyAttribute))) ?? props.First();
            }
            else
            {
                prop = type.GetProperty(PropertyName, _BindingFlags);

                if(prop == null)
                {
                    throw new Exception($"Property with name {PropertyName} does not exist in {typeof(T).Name}");
                }
            }

            var command = OrderByDirection == OrderByDirection.Descending ? "OrderByDescending" : "OrderBy";
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, prop);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, prop.PropertyType },
                                   query.Expression, Expression.Quote(orderByExpression));
            return query.Provider.CreateQuery<T>(resultExpression);

        }
    }
}
