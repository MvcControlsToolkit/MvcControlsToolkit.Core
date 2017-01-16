using System;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations.Queries;

namespace MvcControlsToolkit.Core.Views
{
    public class QuerySearch
    {
        
        public QueryFilterBooleanOperator Value { get; set; }
        public override string ToString()
        {
            if (Value != null) return Value.ToString();
            else return null;
        }

        public bool Allowed(Type t)
        {
            return QueryNodeCache.GetSearchEnabled(t);
            
             
        }
    }
}
