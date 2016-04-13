using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;

namespace MvcControlsToolkit.Core.Validation
{
    public class RangeAttributeAdapterExt: DataAnnotationsClientModelValidator<RangeAttribute>
    {
        public RangeAttributeAdapterExt(RangeAttribute attribute, IStringLocalizer stringLocalizer)
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
            return new[] { new ModelClientValidationRangeRuleExt(errorMessage, Attribute.Minimum, Attribute.Maximum) };
        }
    }
}
