using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreSugar.Filters
{
    public class FilteredQuery<T> where T : class
    {
        public IQueryable<T> Query { get; set; }
        private PagingFilter PagingFilter { get; }
        internal HashSet<string> OrderByProperties { get; } = new HashSet<string>();
        internal List<OrderByFilter> OrderBys { get; } = new List<OrderByFilter>();

        internal FilteredQuery(IQueryable<T> query, PagingFilter pagingFilter, OrderByFilter orderByFilter)
        {
            Query = query;
            PagingFilter = pagingFilter;
            OrderBys.Add(orderByFilter);
        }

        private (IQueryable<T> Query, int RecordCount) BuildPagedQuery()
        {
            var result = Query.Expression;
            var nested = false;

            foreach (var orderByFilter in OrderBys)
            {
                var operation = orderByFilter.ConstructOrderByExpression<T>(nested);
                result = Expression.Call(typeof(Queryable),
                    operation.Command,
                    new[] { Query.ElementType, operation.MemberType },
                    result,
                    Expression.Quote(operation.OrderByExpression));
                nested = true;
            }

            Query = Query.Provider.CreateQuery<T>(result);
            return PagingFilter.ApplyFilter(Query);
        }

        public FilteredResult<T> Resolve()
        {
            var pagingResult = BuildPagedQuery();
            return new FilteredResult<T>() { RecordCount = pagingResult.RecordCount, Value = pagingResult.Query.ToList() };
        }

        public async Task<FilteredResult<T>> ResolveAsync()
        {
            var pagingResult = BuildPagedQuery();
            return new FilteredResult<T>() { RecordCount = pagingResult.RecordCount, Value = await pagingResult.Query.ToListAsync() };
        }

    }
}
