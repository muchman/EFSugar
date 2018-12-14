using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using EFCoreSugar.Global;

namespace EFCoreSugar.Filters
{
    public abstract class Filter
    {
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
        [FilterIgnore]
        public OrderByDirection OrderByDirection { get { return _OrderByFilter.OrderByDirection; } set { _OrderByFilter.OrderByDirection = value; } }
        [FilterIgnore]
        public string OrderByPropertyName { get { return _OrderByFilter.PropertyName; } set { _OrderByFilter.PropertyName = value; } }
        [FilterIgnore]
        public int PageSize { get { return _PagingFilter.PageSize; } set { _PagingFilter.PageSize = value; } }
        [FilterIgnore]
        public int PageNumber { get { return _PagingFilter.PageNumber; } set { _PagingFilter.PageNumber = value; } }
        private Type ThisType { get; }

        public Filter()
        {
            ThisType = this.GetType();
            if (!EFCoreSugarPropertyCollection.FilterTypeProperties.ContainsKey(ThisType))
            {
                EFCoreSugarPropertyCollection.RegisterFilterProperties(ThisType);
            }
        }
        public virtual FilteredQuery<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            //it should be here since we register it in the constructor, or in the Global BuildFilters call
            EFCoreSugarPropertyCollection.FilterTypeProperties.TryGetValue(ThisType, out var filterProps);

            ParameterExpression entityParam = Expression.Parameter(typeof(T));
            Expression<Func<T, bool>> predicate = null;

            foreach (var filterProp in filterProps)
            {
                var propValue = filterProp.Property.GetValue(this);
                if (propValue != null)
                {
                    var propName = filterProp.Attribute?.PropertyName ?? filterProp.Property.Name;

                    //build the predicate.  We walk the string split incase we have a nested property, this way also negates the need to
                    //find the propertyinfo for this thing.  Its less safe but will be much faster
                    var left = (Expression)entityParam;
                    foreach (string name in propName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        left = Expression.PropertyOrField(left, name);
                    }
                    var right = Expression.Constant(propValue);

                    var subPredicate = Expression.Lambda<Func<T, bool>>(
                    FilterTestMap[filterProp.Attribute?.Test ?? FilterTest.Equal](left, right),
                    new[] { entityParam });

                    predicate = predicate != null ? predicate.And(subPredicate) : subPredicate;
                }
            }

            query = query.Where(predicate);
            query = _OrderByFilter.ApplyFilter(query);

            return new FilteredQuery<T>(query, _PagingFilter);
        }
    }
}
