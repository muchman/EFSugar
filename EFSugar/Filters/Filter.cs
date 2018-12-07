using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace EFSugar.Filters
{
    public class Filter
    {
        private const BindingFlags _BindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        internal OrderByDirection OrderByDirection { get { return _OrderByFilter.OrderByDirection; } set { _OrderByFilter.OrderByDirection = value; } }
        internal string PropertyName { get { return _OrderByFilter.PropertyName; } set { _OrderByFilter.PropertyName = value; } }

        private OrderByFilter _OrderByFilter = new OrderByFilter();
        private PagingFilter _PagingFilter = new PagingFilter();


        private static Dictionary<FilterTest, Func<Expression, Expression, BinaryExpression>> FilterTestMap =
            new Dictionary<FilterTest, Func<Expression, Expression, BinaryExpression>>()
            {
                { FilterTest.Equal, Expression.Equal},
                { FilterTest.GreaterThan, Expression.GreaterThan },
                { FilterTest.GreaterThanEqualTo, Expression.GreaterThanOrEqual },
                { FilterTest.LessThan, Expression.LessThan },
                { FilterTest.LessThanEqualTo, Expression.LessThanOrEqual},
                { FilterTest.NotEqual, Expression.NotEqual }
            };

        //string based orderby, there is an extension for this also
        public void SetOrderBy(string propertyName, OrderByDirection direction)
        {
            _OrderByFilter.PropertyName = propertyName;
            _OrderByFilter.OrderByDirection = direction;
        }

        public void SetPaging(int pageNumber, int pageSize) 
        {
            _PagingFilter.PageNumber = pageNumber;
            _PagingFilter.PageSize = pageSize;
        }


        public virtual FilteredQuery<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            ParameterExpression entityParam = Expression.Parameter(typeof(T));
            var expressionGroups = new Dictionary<int, Expression<Func<T, bool>>>();
            var entityType = typeof(T);

            List<Filter> lis = new List<Filter>();

            Expression<Func<T, bool>> predicate = s => true;


            foreach (var prop in this.GetType().GetProperties(_BindingFlags))
            {
                var propValue = prop.GetValue(this);
                if(propValue.IsAssigned(prop))
                {
                    //get the filterproperty
                    var filterAttr = prop.GetCustomAttribute<FilterProperty>() ?? default(FilterProperty);
                    
                    var propName = prop.Name;
                    //see which name to use
                    if (!String.IsNullOrWhiteSpace(filterAttr.PropertyName))
                    {
                        propName = filterAttr.PropertyName;

                    }
                    //get the propertyinfo from the entity
                    var entityProp = entityType.GetProperty(propName, _BindingFlags);

                    if(entityProp != null)
                    {
                        var left = Expression.Property(entityParam, entityProp);
                        var right = Expression.Constant(propValue);

                        var subPredicate = Expression.Lambda<Func<T, bool>>(
                        FilterTestMap[filterAttr.Test](left, right),
                        new[] { entityParam });

                        predicate = filterAttr.Operation == FilterOperation.And ? predicate.And(subPredicate) : predicate.Or(subPredicate);
                    }
                }
            }

            query = query.Where(predicate);
            query = _OrderByFilter.ApplyFilter(query);

            return _PagingFilter.ApplyFilter(query);
        }
    }
}
