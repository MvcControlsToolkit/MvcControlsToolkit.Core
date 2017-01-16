using System;
using System.Linq.Expressions;
using MvcControlsToolkit.Core.DataAnnotations;

namespace MvcControlsToolkit.Core.Views
{
    public class QuerySortingCondition : QueryNode
    {
        public string Property { get; set; }
        public bool Down { get; set; }

        internal LambdaExpression GetSortingLambda(Type t)
        {
            var par = Expression.Parameter(t, "m");
            return Expression.Lambda(BuildAccess(Property, par, t, QueryOptions.OrderBy, "orderby"), par);
        }
        public override string ToString()
        {
            if (Down) return EncodeProperty(Property) + " desc";
            else return EncodeProperty(Property) + " asc";
        }
    }
}
