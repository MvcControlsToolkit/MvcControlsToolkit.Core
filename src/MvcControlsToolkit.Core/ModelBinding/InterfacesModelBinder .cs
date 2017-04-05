using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class InterfacesModelBinder: ComplexTypeModelBinder
    {
        public InterfacesModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinder)
            : base(propertyBinder)
        {

        }
        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            return bindingContext.HttpContext
                .RequestServices.GetService(bindingContext.ModelType);
        }
    }
}
