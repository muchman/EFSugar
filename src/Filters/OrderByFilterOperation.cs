using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EFCoreSugar.Filters
{
    internal class OrderByFilterOperation
    {
        internal string PropertyName { get; }
        internal string Command {get; }
        internal Type MemberType { get; }
        internal LambdaExpression OrderByExpression { get; }

        public OrderByFilterOperation(string propertyName, string command, Type memberType, LambdaExpression orderByExpression)
        {
            Command = command;
            MemberType = memberType;
            OrderByExpression = orderByExpression;
            PropertyName = propertyName;
        }
    }
}
