using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace MvcControlsToolkit.Core.OData
{
    public class EdmClrProperty : EdmStructuralProperty
    {
        
        public EdmClrProperty(EdmClrType declaringType, PropertyInfo property, EdmClrType type)
            : base(declaringType, property.Name, new EdmEntityTypeReference(type, IsNullable(property.PropertyType)))
        {
            
            Property = property;
        }
        
        public EdmClrProperty(EdmClrType declaringType, PropertyInfo property, EdmPrimitiveTypeKind type)
            : base(declaringType, property.Name, EdmCoreModel.Instance.GetPrimitive(type, IsNullable(property.PropertyType)))
        {
            
            Property = property;
        }
        
        public new EdmClrType DeclaringType
            => base.DeclaringType as EdmClrType;
        
        public PropertyInfo Property { get; }
        
        public new EdmClrType Type
            => base.Type as EdmClrType;
        private static bool IsNullable(Type type)
            => !type.GetTypeInfo().IsValueType || (Nullable.GetUnderlyingType(type) != null);
    }
}
