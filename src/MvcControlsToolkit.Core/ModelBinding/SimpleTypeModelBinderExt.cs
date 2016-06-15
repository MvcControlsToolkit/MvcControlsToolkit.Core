using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.Options;
using MvcControlsToolkit.Core.Types;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Mvc.Internal;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class SimpleTypeModelBinderExt: IModelBinder
    {
        private static Type[] allNumbers=new Type[]{typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double) };
        private readonly TypeConverter _typeConverter;

        public SimpleTypeModelBinderExt(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _typeConverter = TypeDescriptor.GetConverter(type);
        }
        private bool needNeutral(ModelBindingContext bindingContext)
        {
            Html5InputSupport support = null;
            try
            {
                support= bindingContext.OperationBindingContext.HttpContext.RequestServices.GetService(typeof(Html5InputSupport)) as Html5InputSupport;
            }
            catch
            {

            }
            if (support == null) return false;
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
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                // no entry
                return TaskCache.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try
            {
                if (needNeutral(bindingContext))
                {
                    valueProviderResult = new ValueProviderResult(valueProviderResult.Values, CultureInfo.InvariantCulture);
                }
                var value = valueProviderResult.FirstValue;

                object model = null;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    model = _typeConverter.ConvertFrom(
                        context: null,
                        culture: valueProviderResult.Culture,
                        value: value);
                }

                if (bindingContext.ModelType == typeof(string))
                {
                    var modelAsString = model as string;
                    if (bindingContext.ModelMetadata.ConvertEmptyStringToNull &&
                        string.IsNullOrEmpty(modelAsString))
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

                    return TaskCache.CompletedTask;
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, model);
                    return TaskCache.CompletedTask;
                }
            }
            catch (Exception exception)
            {
                var isFormatException = exception is FormatException;
                if (!isFormatException && exception.InnerException != null)
                {
                    // TypeConverter throws System.Exception wrapping the FormatException,
                    // so we capture the inner exception.
                    exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                }

                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    exception,
                    bindingContext.ModelMetadata);

                // Were able to find a converter for the type but conversion failed.
                return TaskCache.CompletedTask;
            }
        }
        
    }
}
