using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.Options;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Mvc;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class TransformationModelBinder : IModelBinder
    {
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            return innerBinding(bindingContext);
        }

        private async Task<ModelBindingResult> innerBinding(ModelBindingContext bindingContext)
        {
            var httpContext = bindingContext.OperationBindingContext.HttpContext;
            var tr = httpContext.RequestServices.GetService<RequestTransformationsRegister>();
            if (tr == null) return ModelBindingResult.NoResult;
            string index = tr.GetIndex(bindingContext.ModelName??string.Empty);
            if (index == null) return ModelBindingResult.NoResult;
            Type fctype, fdtype, fitype;
            fctype = TransformationsRegister.InverseTransform(bindingContext.ModelMetadata.ModelType, index, out fitype, out fdtype);
            if(fctype == null) return ModelBindingResult.NoResult;
            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var elementMetadata = metadataProvider.GetMetadataForType(fitype);

            //var innerBindingContext = ModelBindingContext.CreateChildBindingContext(
            //    bindingContext,
            //    elementMetadata,
            //    fieldName: bindingContext.FieldName,
            //    modelName: string.IsNullOrEmpty(bindingContext.ModelName) ? index : bindingContext.ModelName + "." + index,
            //    model: null);
            var modelState = new ModelStateDictionary(bindingContext.ModelState.MaxAllowedErrors);
            string moidelPrefix = string.IsNullOrEmpty(bindingContext.ModelName) ? index : bindingContext.ModelName + "." + index;
            var innerBindingContext = ModelBindingContext.CreateBindingContext(bindingContext.OperationBindingContext, modelState, elementMetadata, null, moidelPrefix);
           
            var result =
                    await bindingContext.OperationBindingContext.ModelBinder.BindModelAsync(innerBindingContext);
            if (result.IsModelSet)
            {
                var validator = httpContext.RequestServices.GetService<IObjectModelValidator>();
                var options = httpContext.ApplicationServices.GetService<IOptions<MvcOptions>>();
                foreach (var prov in options.Value.ModelValidatorProviders) {
                    validator.Validate(prov, modelState, innerBindingContext.ValidationState, moidelPrefix, result.Model);
                }
                foreach(var pair in modelState)
                {
                    bindingContext.ModelState.Add(pair);
                }
                var fres=fctype.GetMethod("InverseTransform").Invoke(Activator.CreateInstance(fctype), new[] { result.Model });
                return ModelBindingResult.Success(bindingContext.ModelName, fres);
               
            }
            return ModelBindingResult.NoResult;
        }
    }
}
