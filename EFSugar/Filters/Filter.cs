using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using EFSugar.Enumerations;

namespace EFSugar.Filters
{
    public class Filter
    {
        private const BindingFlags _BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
        public OrderByDirection OrderByDirection { get { return _OrderByFilter.SortDirection; } set { _OrderByFilter.SortDirection = value; } }
        public int PageNumber { get { return _PagingFilter.PageNumber; } set { _PagingFilter.PageNumber = value; } }
        public int PageSize { get { return _PagingFilter.PageSize; } set { _PagingFilter.PageSize = value; } }

        public string PropertyName { get { return _OrderByFilter.PropertyName; } set { _OrderByFilter.PropertyName = value; } }

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

        public void OrderByProperty(string propertyName,  OrderByDirection direction)
        {
            PropertyName = propertyName;
            OrderByDirection = direction;
        }

        public virtual FilterResult<IQueryable<T>> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            ParameterExpression entityParam = Expression.Parameter(typeof(T));
            var expressionGroups = new Dictionary<int, Expression<Func<T, bool>>>();
            var entityType = typeof(T);

            List<Filter> lis = new List<Filter>();

            Expression<Func<T, bool>> predicate = s => true;


            foreach (var prop in this.GetType().GetProperties(_BindingFlags))
            {
                var filterValue = Convert.ChangeType(prop.GetValue(this), prop.PropertyType);
                if(filterValue.IsAssigned())
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
                        var right = Expression.Constant(filterValue);

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

    //I need a way to self reference the filter.  I cant say this T inside of the filter and I cant expose the properties another way that I know of
    public static class FilterExtension
    {
        public static void OrderByProperty<T>(this T filter, Expression<Func<T, object>> expression, OrderByDirection direction) where T : Filter
        {
            filter.OrderByDirection = direction;

            var unaryExpression = (UnaryExpression)expression.Body;
            var memberExpression = ((MemberExpression)unaryExpression.Operand);

            var filterProperty = memberExpression.Member.GetCustomAttribute<FilterProperty>();

            if(filterProperty != null && !String.IsNullOrWhiteSpace(filterProperty.PropertyName))
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
