using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using EFCoreSugar.Global;
using Microsoft.EntityFrameworkCore;
using EFCoreSugar.Enumerations;
using System.Collections;

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

        private static MethodInfo LikeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        //I made these order and page things public so they can ultimatly be passed into controllers directly by js frameworks without calling additional functions
        [FilterIgnore]
        public OrderByDirection OrderByDirection { get { return _OrderByFilter.OrderByDirection; } set { _OrderByFilter.OrderByDirection = value; } }
        [FilterIgnore]
        public string OrderByPropertyName { get { return _OrderByFilter.PropertyName; } set { _OrderByFilter.PropertyName = value; } }
        [FilterIgnore]
        public int PageSize { get { return _PagingFilter.PageSize; } set { _PagingFilter.PageSize = value; } }
        [FilterIgnore]
        public int PageNumber { get { return _PagingFilter.PageNumber; } set { _PagingFilter.PageNumber = value; } }
        [FilterIgnore]
        public string FuzzyMatchTerm { get; set; }

        private Type ThisType { get; }

        public Filter()
        {
            //save this so we only reflect once
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
            Expression<Func<T, bool>> fuzzyMatchPredicate = null;
            string orderByFinalName = null;
            string fuzzySearchTerm = null;

            if (!string.IsNullOrWhiteSpace(FuzzyMatchTerm))
            {
                if (filterCache.OperationAttribute == null || filterCache.OperationAttribute.FuzzyMode == FuzzyMatchMode.Contains)
                {
                    fuzzySearchTerm = $"%{FuzzyMatchTerm}%";
                }
                else if (filterCache.OperationAttribute.FuzzyMode == FuzzyMatchMode.StartsWith)
                {
                    fuzzySearchTerm = $"{FuzzyMatchTerm}%";
                }
                else //endswith
                {
                    fuzzySearchTerm = $"%{FuzzyMatchTerm}";
                }
            }

            /*
             * using a queue we can build up "look behind" to see how to append the next predicate section.
             * this means we build something like Id && Name && FuzzyMatch1 || FuzzyMatch2 instead of
             * Id && Name || FuzzyMatch1 || Fuzzymatch2
             * */
            Queue<FilterOperation> FilterOperations = new Queue<FilterOperation>();

            foreach (var filterProp in filterCache.FilterProperties)
            {
                var propValue = filterProp.Property.GetValue(this);

                if (!string.IsNullOrWhiteSpace(OrderByPropertyName) && filterProp.Property.Name.Equals(OrderByPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    orderByFinalName = filterProp.PropertyName;
                }

                if (propValue != null || (!string.IsNullOrWhiteSpace(fuzzySearchTerm) && filterProp.Property.PropertyType == typeof(string)))
                {
                    //build the predicate.  We walk the string split incase we have a nested property, this way also negates the need to
                    //find the propertyinfo for this thing.  Its less safe but will be much faster
                    var left = (Expression)entityParam;
                    foreach (string name in filterProp.SplitPropertyName)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(left.Type))
                        {
                            var subtype = left.Type.GetGenericArguments()[0];
                            var newparam = Expression.Parameter(subtype);
                            var newparamexpress = Expression.PropertyOrField(newparam, name);
                            left = Expression.Call(
                                typeof(Enumerable), "Any", new Type[] { subtype },
                                newparamexpress,
                            left);
                        }
                        left = Expression.PropertyOrField(left, name);

                    }
                    Expression right;
                    Expression<Func<T, bool>> subPredicate;

                    //These 2 sections differ only in the right and subpredicate so I just combined them this way
                    if (propValue != null)//we had a value
                    {
                        //we have to do a conversion or else it will blow up when the entity type is nullable
                        right = Expression.Convert(Expression.Constant(propValue), left.Type);

                        subPredicate = Expression.Lambda<Func<T, bool>>(
                        FilterTestMap[filterProp.Test](left, right), new[] { entityParam });

                        FilterOperations.Enqueue(filterProp.Operation);
                    }
                    else//its a fuzzy match
                    {
                        right = Expression.Call(null, LikeMethod, Expression.Constant(EF.Functions), left, Expression.Constant(fuzzySearchTerm));
                        subPredicate = Expression.Lambda<Func<T, bool>>(right, new[] { entityParam });
                        //we always want to OR these together since its a fuzzy match
                        fuzzyMatchPredicate = fuzzyMatchPredicate?.Or(subPredicate) ?? subPredicate;
                        //we just drop out, we are going to hold on to this for the end
                        continue;
                    }

                    if (predicate != null)
                    {
                        if (FilterOperations.Dequeue() == FilterOperation.And)
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
                    throw new Exception($"Cannot find OrderByPropertyName: {OrderByPropertyName} in filter of type: {ThisType}");
                }
            }

            //now we add the fuzzymatch stuff towards the end as a single group so it doesnt mess up groupings
            if (fuzzyMatchPredicate != null)
            {
                //it makes the most sense to AND this on to whatever we have, if those needs change maybe this becomes configurable?
                predicate = predicate?.And(fuzzyMatchPredicate) ?? fuzzyMatchPredicate;
            }

            //if we have not built anything to filter by dont bother making a where clause.
            //I had a default of x => true; thing here but that was trying to resolve outside of the sql server sometimes and that is silly.
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return new FilteredQuery<T>(query, _PagingFilter, _OrderByFilter);
        }
    }
}
