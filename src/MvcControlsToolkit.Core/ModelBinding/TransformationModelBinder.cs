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
        IModelBinder inner;
        IModelMetadataProvider metadataProvider;
        public TransformationModelBinder(Func<ModelMetadata, IModelBinder> getChildBinder, IModelBinder inner, IModelMetadataProvider metadataProvider)
        {
            this.getChildBinder = getChildBinder;
            this.inner = inner;
            this.metadataProvider = metadataProvider;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            Type fctype, fitype, fdtype; string index;
            fctype = getTransformation(bindingContext, out fitype, out fdtype, out index);
            if (fctype == null) return inner.BindModelAsync(bindingContext);
            else return innerBinding(bindingContext, fctype, fdtype, fitype, index);
        }
        private Type getTransformation(ModelBindingContext bindingContext, out Type fitype, out Type fdtype, out string index)
        {
            fitype = null;  fdtype = null; index = null;
            var httpContext = bindingContext.HttpContext;
            var tr = httpContext.RequestServices.GetService<RequestTransformationsRegister>();
            if (tr == null) return null;
            index = tr.GetIndex(bindingContext.ModelName ?? string.Empty);
            if (index == null) return null;
            return TransformationsRegister.InverseTransform(bindingContext.ModelMetadata.ModelType, index, out fitype, out fdtype);
            
        }
        private async Task innerBinding(ModelBindingContext bindingContext, Type fctype, Type fdtype, Type fitype, string index)
        {

            var httpContext = bindingContext.HttpContext;
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
                result = bindingContext.Result;
                childValidationlState = bindingContext.ValidationState;
            }
           
            if (result.IsModelSet)
            {
                var validator = httpContext.RequestServices.GetService<IObjectModelValidator>();
                //var options = httpContext.RequestServices.GetService<IOptions<MvcOptions>>();
                //foreach (var prov in options.Value.ModelValidatorProviders) {
                    validator.Validate(bindingContext.ActionContext, childValidationlState, moidelPrefix, result.Model);
                //}
                //foreach(var pair in modelState)
                //{
                //    bindingContext.ModelState.Add(pair);
                //}
                IBindingTransformation trasf = Activator.CreateInstance(fctype) as IBindingTransformation;
                trasf.Context = httpContext;
                var fres=fctype.GetMethod("InverseTransform").Invoke(trasf, new[] { result.Model });
                bindingContext.Result= ModelBindingResult.Success(fres);
                return;
               
            }
            return;
        }
    }
}
