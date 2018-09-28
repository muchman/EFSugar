using EFSugar.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFSugar
{
    /* Note: You dont need this Expression stuff at all, it is just syntax sugar.  Otherwise you write stuff like:
     *
     * Func<Entity, bool> = o => o.Name != null ? o.Name == "something" : true && o.Age.HasValue? o.Age.Value > 21 : true
     *
     * and you keep building this if(thing) expression else true and honestly that is hard to debug because it is evaluated as a single unit.
     * With this we can actually break up the expressions and only add them when it is required instead of filling in a "else true" just to keep
     * the string of expressions going
     */

    public static class ExpressionExtensions
    {
        //We need this Compose as support for the AND and or OR calls because you cannot just blindly compose to unknown expressions.  More explanation on the ParameterRebinder.
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((paramExpression, index) => new { firstParam = paramExpression, secondParam = second.Parameters[index] }).ToDictionary(p => p.secondParam, p => p.firstParam);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        public static bool IsAssigned<T>(this T item)
        {
            return !(EqualityComparer<T>.Default.Equals(item, default(T)));
        }

        public static bool IsAssigned(this object instance, PropertyInfo prop)
        {
            return prop.GetValue(instance).IsAssigned();
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, SortDirection direction)
        {
            if (direction == SortDirection.Ascending)
            {
                return source.OrderBy(keySelector);
            }
            else
            {
                return source.OrderByDescending(keySelector);
            }
        }
    }

    /* We need this rebinder because if you have an expression that is o => o.Istrue  and b => b.Istrue and you combine them
     * you will end up with o => o.Istrue && b.IsTrue.  The rebinder ensures all the bindings correctly reflect the parameters
     * you are passing in.
     */

    internal class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;

            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }
}
