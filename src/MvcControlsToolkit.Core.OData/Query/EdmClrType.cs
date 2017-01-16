using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace MvcControlsToolkit.Core.OData
{
    public class EdmClrType : EdmEntityType
    {
        
        public EdmClrType(Type type)
            : base(type.Namespace, type.Name)
        {
            Type = type;
        }
        
        public Type Type { get; }
    }
}
