using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreSugar.Filters
{
    public class FilteredQuery<T> where T : class
    {
        public IQueryable<T> Query { get; set; }
        internal Filter ParentFilter { get; }
        internal HashSet<string> OrderByProperties { get; } = new HashSet<string>();
        internal List<OrderByFilter> OrderBys { get; } = new List<OrderByFilter>();

        internal FilteredQuery(IQueryable<T> query, Filter parent)
        {
            Query = query;
            ParentFilter = parent;
        }

        public FilteredResult<T> Resolve()
        {
            var result = Query.Expression;
            var nested = false;

            if(OrderBys.Count == 0)
            {
                OrderBys.Add(ParentFilter._OrderByFilter);
            }

            foreach (var orderByFilter in OrderBys)
            {
                var operation = orderByFilter.ConstructOrderByExpression<T>(nested);
                result = Expression.Call(typeof(Queryable),
                    operation.Command,
                    new[] { Query.ElementType, operation.MemberType},
                    result,
                    Expression.Quote(operation.OrderByExpression));
                nested = true;
            }

            Query = Query.Provider.CreateQuery<T>(result);
            var pagingResult = ParentFilter._PagingFilter.ApplyFilter(Query);
            return new FilteredResult<T>() { RecordCount = pagingResult.RecordCount, Value = pagingResult.Query.ToList() };
        }

    }
}
