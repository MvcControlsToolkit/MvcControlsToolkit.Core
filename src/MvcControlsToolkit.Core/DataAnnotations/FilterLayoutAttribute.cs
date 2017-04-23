using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class FilterLayoutAttribute: Attribute
    {
       
        public QueryOptions[] FilterClauses { get; protected set; }


        public FilterLayoutAttribute(params QueryOptions[] filterClauses)
        {

            FilterClauses = filterClauses;
        }
    }
}
