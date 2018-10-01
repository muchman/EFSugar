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
        private const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
        public Expression OrderBy { get; set; }
        public PagingFilter PagingFilter { get; set; }

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

        public virtual IQueryable<T> ApplyFilter<T>(IQueryable<T> query) where T : class
        {
            ParameterExpression entityParam = Expression.Parameter(typeof(T));
            var expressionGroups = new Dictionary<int, Expression<Func<T, bool>>>();
            var entityType = typeof(T);

            Expression<Func<T, bool>> predicate = s => true;


            foreach (var prop in this.GetType().GetProperties(_bindingFlags))
            {
                var filterValue = prop.GetValue(this);
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
                    var entityProp = entityType.GetProperty(propName, _bindingFlags);

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

            return query.Where(predicate);
        }
    }
}
