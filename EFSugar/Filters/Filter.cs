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
        private OrderByFilter _OrderByFilter = new OrderByFilter();
        private PagingFilter _PagingFilter = new PagingFilter();

        //TODO: rework how this is handled, I didnt want people to import the expression stuff just to define the type of comparar they wanted
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

        //I made these order and page things public so they can ultimatly be passed into controllers directly by js frameworks without calling additional functions
        [ReflectIgnore]
        public OrderByDirection OrderByDirection { get { return _OrderByFilter.OrderByDirection; } set { _OrderByFilter.OrderByDirection = value; } }
        [ReflectIgnore]
        public string OrderByPropertyName { get { return _OrderByFilter.PropertyName; } set { _OrderByFilter.PropertyName = value; } }
        [ReflectIgnore]
        public int PageSize { get { return _PagingFilter.PageSize; } set { _PagingFilter.PageSize = value; } }
        [ReflectIgnore]
        public int PageNumber { get { return _PagingFilter.PageNumber; } set { _PagingFilter.PageNumber = value; } }



        public virtual FilteredQuery<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            ParameterExpression entityParam = Expression.Parameter(typeof(T));
            var expressionGroups = new Dictionary<int, Expression<Func<T, bool>>>();
            var entityType = typeof(T);

            Expression<Func<T, bool>> predicate = null;

            foreach (var prop in this.GetType().GetProperties(_BindingFlags).Where(p => !Attribute.IsDefined(p, typeof(ReflectIgnoreAttribute))))
            {
                var propValue = prop.GetValue(this);
                if (propValue != null)
                {
                    //get the filterproperty
                    var filterAttr = prop.GetCustomAttribute<FilterProperty>() ?? default(FilterProperty);

                    var propName = prop.Name;
                    //see which name to use
                    if (!String.IsNullOrWhiteSpace(filterAttr?.PropertyName))
                    {
                        propName = filterAttr.PropertyName;

                    }

                    //build the predicate.  We walk the string split incase we have a nested property, this way also negates the need to
                    //find the propertyinfo for this thing.  Its less safe but will be much faster
                    var left = (Expression)entityParam;
                    foreach (string name in propName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        left = Expression.PropertyOrField(left, name);
                    }
                    var right = Expression.Constant(propValue);

                    var subPredicate = Expression.Lambda<Func<T, bool>>(
                    FilterTestMap[filterAttr?.Test ?? FilterTest.Equal](left, right),
                    new[] { entityParam });

                    predicate = predicate != null ? predicate.And(subPredicate) : subPredicate;

                }
            }

            query = query.Where(predicate);
            query = _OrderByFilter.ApplyFilter(query);

            return _PagingFilter.ApplyFilter(query);
        }
    }
}
