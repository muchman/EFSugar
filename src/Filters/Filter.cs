using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using EFCoreSugar.Global;
using Microsoft.EntityFrameworkCore;
using EFCoreSugar.Enumerations;

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
        [FilterIgnore]
        public string FuzzySearchTerm { get; set; }

        private static MethodInfo LikeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });

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
            EFCoreSugarPropertyCollection.FilterTypeProperties.TryGetValue(ThisType, out var filterCache);

            var type = typeof(T);
            ParameterExpression entityParam = Expression.Parameter(type);
            Expression<Func<T, bool>> predicate = null;
            string orderByFinalName = null;
            string fuzzySearchTerm = null;

            if(!string.IsNullOrWhiteSpace(FuzzySearchTerm))
            {
                if(filterCache.OperationAttribute == null || filterCache.OperationAttribute.FuzzyMode == FuzzySearchMode.Contains)
                {
                    fuzzySearchTerm = $"%{FuzzySearchTerm}%";
                }
                else if(filterCache.OperationAttribute.FuzzyMode == FuzzySearchMode.StartsWith)
                {
                    fuzzySearchTerm = $"{FuzzySearchTerm}%";
                }
                else
                {
                    fuzzySearchTerm = $"%{FuzzySearchTerm}";
                }
            }

            foreach (var filterProp in filterCache.FilterProperties)
            {
                var propValue = filterProp.Property.GetValue(this);
                var propName = filterProp.PropertyAttribute?.PropertyName ?? filterProp.Property.Name;

                if (!string.IsNullOrWhiteSpace(OrderByPropertyName) && filterProp.Property.Name.Equals(OrderByPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    orderByFinalName = propName;
                }

                if (propValue != null)
                {
                    //build the predicate.  We walk the string split incase we have a nested property, this way also negates the need to
                    //find the propertyinfo for this thing.  Its less safe but will be much faster
                    var left = (Expression)entityParam;
                    foreach (string name in propName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        left = Expression.PropertyOrField(left, name);
                    }
                    var right = Expression.Constant(propValue);

                    var subPredicate = Expression.Lambda<Func<T, bool>>(
                    FilterTestMap[filterProp.PropertyAttribute?.Test ?? FilterTest.Equal](left, right),
                    new[] { entityParam });

                    if(predicate != null)
                    {
                        if(filterProp.Operation == FilterOperation.And)
                        {
                            predicate = predicate.And(subPredicate);
                        }
                        else
                        {
                            predicate = predicate.Or(subPredicate);
                        }
                    }
                    else
                    {
                        predicate = subPredicate;
                    }                   
                }
                else if (!string.IsNullOrWhiteSpace(fuzzySearchTerm) && filterProp.Property.PropertyType == typeof(string))
                {
                    var left = (Expression)entityParam;
                    foreach (string name in propName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        left = Expression.PropertyOrField(left, name);
                    }
                    var right = Expression.Call(null, LikeMethod, Expression.Constant(EF.Functions), left, Expression.Constant(fuzzySearchTerm));
                    var subPredicate = Expression.Lambda<Func<T, bool>>(right, new[] { entityParam });
                    if (predicate != null)
                    {
                        if (filterProp.Operation == FilterOperation.And)
                        {
                            predicate = predicate.And(subPredicate);
                        }
                        else
                        {
                            predicate = predicate.Or(subPredicate);
                        }
                    }
                    else
                    {
                        predicate = subPredicate;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(OrderByPropertyName))
            {
                if (orderByFinalName != null)
                {
                    OrderByPropertyName = orderByFinalName;
                }
                else
                {
                    throw new Exception($"Cannot find OrderByPropertName: {OrderByPropertyName} in filter of type: {ThisType}");
                }
            }

            predicate = predicate ?? Expression.Lambda<Func<T, bool>>(Expression.Constant(true), Expression.Parameter(type));

            query = query.Where(predicate);

            return new FilteredQuery<T>(query, _PagingFilter, _OrderByFilter);
        }
    }
}
