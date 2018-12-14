using EFCoreSugar.Global;
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
        public OrderByDirection OrderByDirection { get; set; }
        public string PropertyName { get; set; }
        public IQueryable<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            var operation = ConstructOrderByExpression<T>();

            var result = Expression.Call(typeof(Queryable),
                operation.Command,
                new[] { query.ElementType, operation.MemberType },
                query.Expression,
                Expression.Quote(operation.OrderByExpression));

            return query.Provider.CreateQuery<T>(result);
        }

        internal OrderByFilterOperation ConstructOrderByExpression<T>(bool nested = false) where T : class
        {
            var type = typeof(T);
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                PropertyName = EFCoreSugarPropertyCollection.GetDefaultPropertyName(type);
            }

            var command = OrderByDirection == OrderByDirection.Descending ? nested ? "ThenByDescending" : "OrderByDescending" : nested ? "ThenBy" : "OrderBy";
            var parameter = Expression.Parameter(type, "p");
            Expression memberAccess = null;

            foreach (var property in PropertyName.Split('.'))
            {
                memberAccess = Expression.Property(memberAccess ?? parameter, property);
            }

            return new OrderByFilterOperation(PropertyName, command, memberAccess.Type, Expression.Lambda(memberAccess, parameter));
        }
    }
}
