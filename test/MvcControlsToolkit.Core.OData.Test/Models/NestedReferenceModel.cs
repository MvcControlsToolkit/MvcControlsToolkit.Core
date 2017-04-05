using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.OData.Test.Models
{
    public class NestedReferenceModel
    {
        public int Id { get; set; }
        public string AString { get; set; }
        public int AInt { get; set; }

        public virtual ReferenceModel Father { get; set; }

        public int FatherId { get; set; }
    }
}
