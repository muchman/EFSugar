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
        private static MethodInfo BuildPredicateMethod = typeof(Filter).GetMethod("BuildPredicate", BindingFlags.Instance | BindingFlags.NonPublic);

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

        /*
         * using a queue we can build up "look behind" to see how to append the next predicate section.
         * this means we build something like Id && Name && FuzzyMatch1 || FuzzyMatch2 instead of
         * Id && Name || FuzzyMatch1 || Fuzzymatch2
         * */
        private Queue<FilterOperation> FilterOperations = new Queue<FilterOperation>();

        public Filter()
        {
            //save this so we only reflect once
            ThisType = this.GetType();

            if (!EFCoreSugarPropertyCollection.FilterTypeProperties.ContainsKey(ThisType))
            {
                EFCoreSugarPropertyCollection.RegisterFilterProperties(ThisType);
            }
        }

        private Expression<Func<T, bool>> BuildPredicate<T>(ParameterExpression predParam, Expression currentLeft, FilterProperty filterProp, object propValue, bool fuzzyMatch, int index = 0) where T : class
        {
            for (;index < filterProp.SplitPropertyName.Length; index++)
            {
                var name = filterProp.SplitPropertyName[index];
                if (typeof(IEnumerable).IsAssignableFrom(currentLeft.Type) && currentLeft.Type != typeof(string))
                {
                    var subtype = currentLeft.Type.GetGenericArguments()[0];
                    var method = BuildPredicateMethod.MakeGenericMethod(subtype);
                    var newParam = Expression.Parameter(subtype);
                    var predExpr = (Expression)method.Invoke(this, new object[] { newParam, newParam, filterProp, propValue, fuzzyMatch, index });
                    currentLeft = Expression.Call(
                        typeof(Enumerable), "Any", new Type[] { subtype },
                        currentLeft,
                        predExpr
                    );
                    return Expression.Lambda<Func<T, bool>>(currentLeft, new[] { predParam });
                }
                //else
                currentLeft = Expression.PropertyOrField(currentLeft, name);
            }

            Expression<Func<T, bool>> subPredicate;

            //we build the right differently for fuzzy vs not
            if (!fuzzyMatch)
            {
                //we have to do a conversion or else it will blow up when the entity type is nullable
                var right = Expression.Convert(Expression.Constant(propValue), currentLeft.Type);

                subPredicate = Expression.Lambda<Func<T, bool>>(
                FilterTestMap[filterProp.Test](currentLeft, right), new[] { predParam });
                //we enqueue the filteroperations to do "look back" style of operations
                FilterOperations.Enqueue(filterProp.Operation);
            }
            else//its a fuzzy match
            {
                var right = Expression.Call(null, LikeMethod, Expression.Constant(EF.Functions), currentLeft, Expression.Constant(propValue));
                subPredicate = Expression.Lambda<Func<T, bool>>(right, new[] { predParam });
            }
            return subPredicate;
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
            string fuzzyMatchTerm = null;

            if (!string.IsNullOrWhiteSpace(FuzzyMatchTerm))
            {
                if (filterCache.OperationAttribute == null || filterCache.OperationAttribute.FuzzyMode == FuzzyMatchMode.Contains)
                {
                    fuzzyMatchTerm = $"%{FuzzyMatchTerm}%";
                }
                else if (filterCache.OperationAttribute.FuzzyMode == FuzzyMatchMode.StartsWith)
                {
                    fuzzyMatchTerm = $"{FuzzyMatchTerm}%";
                }
                else //endswith
                {
                    fuzzyMatchTerm = $"%{FuzzyMatchTerm}";
                }
            }

            foreach (var filterProp in filterCache.FilterProperties)
            {
                if (!string.IsNullOrWhiteSpace(OrderByPropertyName) && filterProp.Property.Name.Equals(OrderByPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    orderByFinalName = filterProp.PropertyName;
                }

                var propValue = filterProp.Property.GetValue(this);

                if (propValue != null)
                {
                    //build the predicate.  We walk the string split incase we have a nested property, this way also negates the need to
                    //find the propertyinfo for this thing.  Its less safe but will be much faster
                    var subPredicate = BuildPredicate<T>(entityParam, entityParam, filterProp, propValue, false);
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
                else if(!string.IsNullOrWhiteSpace(fuzzyMatchTerm) && filterProp.Property.PropertyType == typeof(string))
                {
                    var subPredicate = BuildPredicate<T>(entityParam, entityParam, filterProp, fuzzyMatchTerm, true);
                    fuzzyMatchPredicate = fuzzyMatchPredicate?.Or(subPredicate) ?? subPredicate;
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
