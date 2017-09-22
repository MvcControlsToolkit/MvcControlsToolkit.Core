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
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) };
        private readonly TypeConverter _typeConverter;

        public SimpleTypeModelBinderExt(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _typeConverter = TypeDescriptor.GetConverter(type);
        }
        private bool needNeutral(ModelBindingContext bindingContext, out Html5InputSupport support)
        {
            support = null;
            try
            {
                support= bindingContext.HttpContext.RequestServices.GetService(typeof(Html5InputSupport)) as Html5InputSupport;
            }
            catch(Exception ex)
            {
                var l= ex;
            }
            if (support == null) return false;
            var type = bindingContext.ModelMetadata.UnderlyingOrModelType;
            if (allNumbers.Contains(type))
            {
                var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName.Length > 0 ? bindingContext.ModelName+"._" : "_");
                return valueProviderResult == ValueProviderResult.None ?  support.Number > 2 : support.Range > 2;
            }
            
            if (type == typeof(DateTime))
            {
                if (bindingContext.ModelMetadata.DataTypeName == "Date") return support.Date > 2;
                else if (bindingContext.ModelMetadata.DataTypeName == "Time") return support.Time > 2;
                else return support.DateTime > 2;
            }
            if (type == typeof(DateTimeOffset)) return support.DateTime > 2;
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
#if        NET451
                return TaskCache.CompletedTask;
#else
                return Task.CompletedTask;
#endif
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try
            {
                Html5InputSupport support;
                if (needNeutral(bindingContext, out support))
                {
                    valueProviderResult = new ValueProviderResult(valueProviderResult.Values, CultureInfo.InvariantCulture);
                }
                var value = valueProviderResult.FirstValue;

                object model = null;
                
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (allNumbers.Contains(bindingContext.ModelMetadata.UnderlyingOrModelType))
                    {
                        value = value.Replace(valueProviderResult.Culture.NumberFormat.NumberGroupSeparator, string.Empty);
                    }
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
                else if (model != null && bindingContext.ModelMetadata.UnderlyingOrModelType == typeof(DateTimeOffset))
                {
                    
                    var offsetValueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName+".O");
                    double offset = 0;
                    if(offsetValueProviderResult != ValueProviderResult.None)
                    {
                        var ovalue = offsetValueProviderResult.FirstValue;
                        if(ovalue!= null) double.TryParse(ovalue, NumberStyles.Number, CultureInfo.InvariantCulture, out offset);
                    }
                    if(offset >1 && offset<-1) bindingContext.ModelState.Remove(bindingContext.ModelName);
                    var dModel = (DateTimeOffset)model;
                    model = dModel.AddMinutes(dModel.Offset.TotalMinutes - offset);
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

#if NET451
                    return TaskCache.CompletedTask;
#else
                return Task.CompletedTask;
#endif
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(model);
#if NET451
                    return TaskCache.CompletedTask;
#else
                return Task.CompletedTask;
#endif
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
#if NET451
                return TaskCache.CompletedTask;
#else
                return Task.CompletedTask;
#endif
            }
        }
        
    }
}
