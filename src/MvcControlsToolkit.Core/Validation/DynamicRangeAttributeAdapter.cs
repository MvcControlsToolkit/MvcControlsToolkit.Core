using System;
using System.Collections.Generic;
using System.Globalization;
using MvcControlsToolkit.Core.Types;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MvcControlsToolkit.Core.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;

namespace MvcControlsToolkit.Core.Validation
{
    public class DynamicRangeAttributeAdapter: AttributeAdapterBase<DynamicRangeAttribute>
    {
        public DynamicRangeAttributeAdapter(DynamicRangeAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        {

        }
        public override void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var errorMessage = GetErrorMessage(context);
            var fatherModel = context.ModelMetadata.PropertyGetter.Target;
            List<string> clientMaxs, clientMaxDelays, clientMins, clientMinDelays;
            var maxValue = Attribute.GetGlobalMaximum(fatherModel, out clientMaxs, out clientMaxDelays);
            var minValue = Attribute.GetGlobalMinimum(fatherModel, out clientMins, out clientMinDelays);
            if (minValue != null || maxValue != null)
            {
                
                if (minValue != null)
                {
                    if (minValue is DateTime) minValue = ((DateTime)minValue).ToString("s", CultureInfo.InvariantCulture);
                    else if (minValue is Week)
                    {
                        var date = ((Week)minValue).StartDate();
                        minValue = ((Week)minValue).ToString();
                    }
                    else if (minValue is Month)
                    {
                        var date = ((Month)minValue).ToDateTime();
                        minValue = ((Month)minValue).ToString();
                    }
                    else if (minValue is IConvertible) minValue = (minValue as IConvertible).ToString(CultureInfo.InvariantCulture);
                    else if (minValue is IFormattable) minValue = (minValue as IFormattable).ToString(null, CultureInfo.InvariantCulture);
                }
                if (maxValue != null)
                {
                    if (maxValue is DateTime) maxValue = ((DateTime)maxValue).ToString("s", CultureInfo.InvariantCulture);
                    else if (maxValue is Week)
                    {
                        var date = ((Week)maxValue).StartDate();
                        maxValue = ((Week)maxValue).ToString();
                    }
                    else if (maxValue is Month)
                    {
                        var date = ((Month)maxValue).ToDateTime();
                        maxValue = ((Month)maxValue).ToString();
                    }
                    else if (maxValue is IConvertible) maxValue = (maxValue as IConvertible).ToString(CultureInfo.InvariantCulture);
                    else if (maxValue is IFormattable) maxValue = (maxValue as IFormattable).ToString(null, CultureInfo.InvariantCulture);
                }

                MergeAttribute(context.Attributes, "data-val", "true");
                MergeAttribute(context.Attributes, "data-val-range", errorMessage);
                MergeAttribute(context.Attributes, "data-val-range-min", minValue as string);
                MergeAttribute(context.Attributes, "data-val-range-max", maxValue as string);
                MergeAttribute(context.Attributes, "data-val-range-go", Attribute.Propagate ? "true" : "false");
            }
            if ((clientMaxs != null && clientMaxs.Count > 0) || (clientMaxDelays != null && clientMaxDelays.Count > 0) || (clientMins != null && clientMins.Count > 0) || (clientMinDelays != null && clientMinDelays.Count > 0))
            {
                

                MergeAttribute(context.Attributes, "data-val", "true");
                MergeAttribute(context.Attributes, "data-val-drange", errorMessage);

                MergeAttribute(context.Attributes, "data-val-drange-dmins", clientMins == null ? string.Empty : String.Join(" ", clientMins));
                MergeAttribute(context.Attributes, "data-val-drange-dminds", clientMinDelays == null ? string.Empty : String.Join(" ", clientMinDelays));

                MergeAttribute(context.Attributes, "data-val-drange-dmaxs", clientMaxs == null ? string.Empty : String.Join(" ", clientMaxs));
                MergeAttribute(context.Attributes, "data-val-drange-dmaxds", clientMaxDelays == null ? string.Empty : String.Join(" ", clientMaxDelays));

                MergeAttribute(context.Attributes, "data-val-drange-go", Attribute.Propagate ? "true" : "false");
            }
            
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            return GetErrorMessage(validationContext.ModelMetadata,
                validationContext.ModelMetadata.GetDisplayName(),
                Attribute.Minimum,
                Attribute.Maximum);  
        }

    }
}
