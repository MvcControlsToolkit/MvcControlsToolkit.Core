using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using MvcControlsToolkit.Core.DataAnnotations;
namespace MvcControlsToolkit.Core.Validation
{
    public class DynamicRangeAttributeAdapter: DataAnnotationsClientModelValidator<DynamicRangeAttribute>
    {
        public DynamicRangeAttributeAdapter(DynamicRangeAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        {

        }
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var errorMessage = GetErrorMessage(context.ModelMetadata);
            var fatherModel = context.ModelMetadata.PropertyGetter.Target;
            List<string> clientMaxs, clientMaxDelays, clientMins, clientMinDelays;
            var max = Attribute.GetGlobalMaximum(fatherModel, out clientMaxs, out clientMaxDelays);
            var min = Attribute.GetGlobalMinimum(fatherModel, out clientMins, out clientMinDelays);
            List<ModelClientValidationRule> res = new List<ModelClientValidationRule>();
            if (min != null || max != null) res.Add(new ModelClientValidationRangeRuleExt(errorMessage, min, max));
            if ((clientMaxs != null && clientMaxs.Count>0) || (clientMaxDelays != null && clientMaxDelays.Count>0) || (clientMins != null && clientMins.Count>0) || (clientMinDelays != null && clientMinDelays.Count>0))
            {
                res.Add(new ModelClientValidationDRangeRule(errorMessage, clientMins, clientMinDelays, clientMaxs, clientMaxDelays));
            }
            return res;
        }
    }
}
