using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.Options;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;


namespace MvcControlsToolkit.Core.ModelBinding
{
    public class TransformationModelBinder : IModelBinder
    {
        Func<ModelMetadata, IModelBinder> getChildBinder;
        public TransformationModelBinder(Func<ModelMetadata, IModelBinder> getChildBinder)
        {
            this.getChildBinder = getChildBinder;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return innerBinding(bindingContext);
        }

        private async Task innerBinding(ModelBindingContext bindingContext)
        {
            var httpContext = bindingContext.OperationBindingContext.HttpContext;
            var tr = httpContext.RequestServices.GetService<RequestTransformationsRegister>();
            if (tr == null) return;
            string index = tr.GetIndex(bindingContext.ModelName??string.Empty);
            if (index == null) return;
            Type fctype, fdtype, fitype;
            fctype = TransformationsRegister.InverseTransform(bindingContext.ModelMetadata.ModelType, index, out fitype, out fdtype);
            if (fctype == null) return;
            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var elementMetadata = metadataProvider.GetMetadataForType(fitype);

            
            string moidelPrefix = string.IsNullOrEmpty(bindingContext.ModelName) ? index : bindingContext.ModelName + "." + index;
            string fieldName = bindingContext.FieldName;
            ModelBindingResult result;
            ValidationStateDictionary childValidationlState;
            using (bindingContext.EnterNestedScope(
                modelMetadata: elementMetadata,
                fieldName: fieldName,
                modelName: moidelPrefix,
                model: null))
            {
                await getChildBinder(elementMetadata).BindModelAsync(bindingContext);
                result = bindingContext.Result ?? ModelBindingResult.Failed(moidelPrefix);
                childValidationlState = bindingContext.ValidationState;
            }
           
            if (result.IsModelSet)
            {
                var validator = httpContext.RequestServices.GetService<IObjectModelValidator>();
                var options = httpContext.RequestServices.GetService<IOptions<MvcOptions>>();
                foreach (var prov in options.Value.ModelValidatorProviders) {
                    validator.Validate(bindingContext.OperationBindingContext.ActionContext, prov, childValidationlState, moidelPrefix, result.Model);
                }
                //foreach(var pair in modelState)
                //{
                //    bindingContext.ModelState.Add(pair);
                //}
                var fres=fctype.GetMethod("InverseTransform").Invoke(Activator.CreateInstance(fctype), new[] { result.Model });
                bindingContext.Result= ModelBindingResult.Success(bindingContext.ModelName, fres);
                return;
               
            }
            return ;
        }
    }
}
