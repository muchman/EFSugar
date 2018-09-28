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
        private const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
        public Expression OrderBy { get; set; }
        public PagingFilter PagingFilter { get; set; }

        public string Origina_Address { get; set; }

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
                    var filterAttr = prop.GetCustomAttribute<FilterProperty>();
                    
                    var propName = prop.Name;
                    //see which name to use
                    if (filterAttr.IsAssigned() && !String.IsNullOrWhiteSpace(filterAttr.PropertyName))
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
                        Expression.Equal(left, right),
                        new[] { entityParam });

                        predicate = predicate.And(subPredicate);
                    }
                }
            }

            return query.Where(predicate);
        }
    }
}
