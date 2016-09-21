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
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;

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
            if(fitype == null)
            {
                fitype=fctype;
                fctype = null;
            }
            return innerBinding(bindingContext, fctype, fdtype, fitype, index);
        }
        private Type getTransformation(ModelBindingContext bindingContext, out Type fitype, out Type fdtype, out string index)
        {
            fitype = null;  fdtype = null; index = null;
            var httpContext = bindingContext.HttpContext;
            var tr = httpContext.RequestServices.GetService<RequestTransformationsRegister>();
            if (tr == null) return null;
            index = tr.GetIndex(bindingContext.ModelName ?? string.Empty);
            if (index == null) return null;
            if(index.Length>0 && index[index.Length - 1] == '_')
            {
                return DerivedClassesRegister.GetTypeFromCode(index);
            }
            else return TransformationsRegister.InverseTransform(bindingContext.ModelMetadata.ModelType, index, out fitype, out fdtype);
            
        }
        private async Task innerBinding(ModelBindingContext bindingContext, Type fctype, Type fdtype, Type fitype, string index)
        {

            var httpContext = bindingContext.HttpContext;
            var elementMetadata = metadataProvider.GetMetadataForType(fitype);
            object model = null;
            if (fctype == null)//subclass! First bind main class, 
                //and then extra properties defined in subclass
            {
                bindingContext.Model = Activator.CreateInstance(fitype);
                await inner.BindModelAsync(bindingContext);
                if (!bindingContext.Result.IsModelSet) return;
                model = bindingContext.Result.Model;
                bindingContext.Result = new ModelBindingResult();
                elementMetadata = new DiffMetaData(elementMetadata, bindingContext.ModelMetadata);
            }


            string modelPrefix = string.IsNullOrEmpty(bindingContext.ModelName) ? index : bindingContext.ModelName + "." + index;
            string fieldName = bindingContext.FieldName;
            ModelBindingResult result;
            ValidationStateDictionary childValidationlState;
            using (bindingContext.EnterNestedScope(
                modelMetadata: elementMetadata,
                fieldName: fieldName,
                modelName: modelPrefix,
                model: model))
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
                    validator.Validate(bindingContext.ActionContext, childValidationlState, modelPrefix, result.Model);
                //}
                //foreach(var pair in modelState)
                //{
                //    bindingContext.ModelState.Add(pair);
                //}
                if (fctype != null)
                {
                    IBindingTransformation trasf = Activator.CreateInstance(fctype) as IBindingTransformation;
                    trasf.Context = httpContext;
                    var fres = fctype.GetMethod("InverseTransform").Invoke(trasf, new[] { result.Model });
                    bindingContext.Result = ModelBindingResult.Success(fres);
                    return;
                }//no trasformation result.Model is a subclass of the required type
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(result.Model);
                    return;
                }
               
            }
            return;
        }
    }
}
