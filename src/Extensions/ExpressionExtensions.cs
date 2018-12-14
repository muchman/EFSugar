using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFCoreSugar.Filters
{
    /* Note: Yes I have commented code here, it will be removed soon!

     */

    public static class ExpressionExtensions
    {
        //We need this Compose as support for the AND and or OR calls because you cannot just blindly compose to unknown expressions.  More explanation on the ParameterRebinder.
        internal static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
           // build parameter map (from parameters of second to parameters of first)
           // var map = first.Parameters.Select((paramExpression, index) => new { firstParam = paramExpression, secondParam = second.Parameters[index] }).ToDictionary(p => p.secondParam, p => p.firstParam);

            // replace parameters in the second lambda expression with parameters from the first

            // apply composition of lambda expression bodies to parameters from the first expression
            return Expression.Lambda<T>(merge(first.Body, second.Body), first.Parameters);
        }

        internal static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        internal static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        internal static bool IsAssigned<T>(this T item)
        {
            return !(EqualityComparer<T>.Default.Equals(item, default(T)));
        }

        internal static bool IsAssigned<T>(this T item, PropertyInfo prop)
        {
            if (prop.PropertyType.GetConstructor(Type.EmptyTypes) == null)
            {
                var value = Activator.CreateInstance(prop.PropertyType);
                return !(Equals(item, value));
            }
            else
            {
                return item != null;
            }
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, OrderByDirection direction)
        {
            if (direction == OrderByDirection.Ascending)
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

    //internal class BasicRebinder : ExpressionVisitor
    //{
    //    private readonly ParameterExpression _parameter;

    //    protected override Expression VisitParameter(ParameterExpression node)
    //    {
    //        return base.VisitParameter(_parameter);
    //    }

    //    internal BasicRebinder(ParameterExpression parameter)
    //    {
    //        _parameter = parameter;
    //    }
    //}

    //internal class ParameterRebinder : ExpressionVisitor
    //{
    //    private readonly Dictionary<ParameterExpression, ParameterExpression> map;

    //    public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
    //    {
    //        this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
    //    }

    //    public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
    //    {
    //        return new ParameterRebinder(map).Visit(exp);
    //    }

    //    protected override Expression VisitParameter(ParameterExpression p)
    //    {
    //        ParameterExpression replacement;

    //        if (map.TryGetValue(p, out replacement))
    //        {
    //            p = replacement;
    //        }

    //        return base.VisitParameter(p);
    //    }
    //}
}
