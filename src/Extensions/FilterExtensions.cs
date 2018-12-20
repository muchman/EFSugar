using EFCoreSugar.Global;
using EFCoreSugar.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Filters
{

    //I need a way to self reference the filter.  I cant say this T inside of the filter and I cant expose the properties another way that I know of
    //Since filter properties are not known to use, this was the only way to allow the type to be generic without affecting the base class
    public static class FilterExtensions
    {

        public static void SetOrderBy<T>(this T filter, Expression<Func<T, object>> expression, OrderByDirection direction = OrderByDirection.Ascending) where T : Filter
        {
            filter.OrderByDirection = direction;

            var unaryExpression = (UnaryExpression)expression.Body;
            var memberExpression = ((MemberExpression)unaryExpression.Operand);

            filter.OrderByPropertyName = memberExpression.Member.Name;           
        }

        public static FilteredQuery<T> Filter<T>(this IQueryable<T> query, Filter filter) where T : class
        {
            return filter.ApplyFilter(query);
        }

        public static FilteredQuery<T> Filter<T>(this DbContext context, Filter filter) where T : class
        {
            var query = from data in context.Set<T>() select data;
            return query.Filter(filter);
        }

        public static FilteredQuery<T> Filter<T>(this FilteredQuery<T> baseQuery, Filter filter) where T : class
        {
            //we will preserve the first filter paging settings always
            var newFilteredQuery = filter.ApplyFilter(baseQuery.Query);
            baseQuery.Query = newFilteredQuery.Query;

            if (baseQuery.OrderByProperties.Count == 0)
            {
                baseQuery.OrderByProperties.Add(baseQuery.OrderBys.First().PropertyName ?? EFCoreSugarPropertyCollection.GetDefaultPropertyName(typeof(T)));
            }

            //we want to ignore additional orderbys if they are not set explicitly, this likely means they are doing something else and dont care about the order by
            var newOrderBy = newFilteredQuery.OrderBys.First();
            if (newOrderBy.PropertyName != null && !baseQuery.OrderByProperties.Contains(newOrderBy.PropertyName))
            {
                baseQuery.OrderBys.Add(newOrderBy);
                baseQuery.OrderByProperties.Add(newOrderBy.PropertyName);
            }
            return baseQuery;
        }

        public static FilteredResult<T> Filter<T>(this IBaseDbRepository repository, Filter filter) where T : class
        {
            return repository.DBContext.Filter<T>(filter).Resolve();
        }
    }
}
