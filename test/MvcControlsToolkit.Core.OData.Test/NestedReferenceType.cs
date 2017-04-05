using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.OData.Test
{
    public interface IFilterNestedReferenceType
    {
        int? Id { get; set; }
        int AInt { get; set; }
    }
    public class NestedReferenceType: IFilterNestedReferenceType
    {
        public int? Id { get; set; }
        public string AString { get; set; }
        public int AInt { get; set; }
    }
}
