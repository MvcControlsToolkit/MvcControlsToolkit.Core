using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.Options;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class SimpleTypeModelBinderExt: IModelBinder
    {
        private static Type[] allNumbers=new Type[]{typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double) };
        private bool needNeutral(ModelBindingContext bindingContext)
        {
            var support = bindingContext.OperationBindingContext.HttpContext.RequestServices.GetRequiredService<Html5InputSupport>();
            var type = bindingContext.ModelMetadata.UnderlyingOrModelType;
            if (allNumbers.Contains(type)) return support.Number>2;
            
            if (type == typeof(DateTime))
            {
                if (bindingContext.ModelMetadata.DataTypeName == "Date") return support.Date > 2;
                else if (bindingContext.ModelMetadata.DataTypeName == "Time") return support.Time > 2;
                else return support.DateTime > 2;
            }
            if (type == typeof(Week)) return support.Week > 2;
            if (type == typeof(Month)) return support.Month > 2;
            if(type == typeof(TimeSpan) && bindingContext.ModelMetadata.DataTypeName == "Time") return support.Time > 2;
            return false;
        }
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            // This method is optimized to use cached tasks when possible and avoid allocating
            // using Task.FromResult. If you need to make changes of this nature, profile
            // allocations afterwards and look for Task<ModelBindingResult>.

            if (bindingContext.ModelMetadata.IsComplexType)
            {
                // this type cannot be converted
                return ModelBindingResult.NoResultAsync;
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                // no entry
                return ModelBindingResult.NoResultAsync;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try
            {
                if (needNeutral(bindingContext))
                {
                    valueProviderResult = new ValueProviderResult(valueProviderResult.Values, CultureInfo.InvariantCulture);
                }
                var model = valueProviderResult.ConvertTo(bindingContext.ModelType);

                if (bindingContext.ModelType == typeof(string))
                {
                    var modelAsString = model as string;
                    if (bindingContext.ModelMetadata.ConvertEmptyStringToNull &&
                        string.IsNullOrWhiteSpace(modelAsString))
                    {
                        model = null;
                    }
                }

                // When converting newModel a null value may indicate a failed conversion for an otherwise required
                // model (can't set a ValueType to null). This detects if a null model value is acceptable given the
                // current bindingContext. If not, an error is logged.
                if (model == null && !bindingContext.ModelMetadata.IsReferenceOrNullableType)
                {
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                            valueProviderResult.ToString()));

                    return ModelBindingResult.FailedAsync(bindingContext.ModelName);
                }
                else
                {
                    return ModelBindingResult.SuccessAsync(bindingContext.ModelName, model);
                }
            }
            catch (Exception exception)
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    exception,
                    bindingContext.ModelMetadata);

                // Were able to find a converter for the type but conversion failed.
                // Tell the model binding system to skip other model binders.
                return ModelBindingResult.FailedAsync(bindingContext.ModelName);
            }
        }
    }
}
