using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using System.Globalization;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.Validation
{
    public class ModelClientValidationRangeRuleExt: ModelClientValidationRule
    {
        private const string RangeValidationType = "range";
        private const string MinValidationParameter = "min";
        private const string MaxValidationParameter = "max";

        public ModelClientValidationRangeRuleExt(
            string errorMessage,
            object minValue,
            object maxValue)
            : base(RangeValidationType, errorMessage)
        {
            if (minValue == null && maxValue == null)
            {
                throw new ArgumentNullException(nameof(minValue)+" or "+ nameof(maxValue));
            }
            if (minValue != null)
            {
                if (minValue is DateTime) minValue = ((DateTime)minValue).ToString("s", CultureInfo.InvariantCulture);
                else if (minValue is Week)
                {
                    var date = ((Week)minValue).StartDate();
                    minValue = ((Week)minValue).ToString() + "|" + date.Month + "|" + date.Day;
                }
                else if (minValue is IConvertible) minValue = (minValue as IConvertible).ToString(CultureInfo.InvariantCulture);
            }
            if (maxValue != null)
            {
                if (maxValue is DateTime) maxValue = ((DateTime)maxValue).ToString("s", CultureInfo.InvariantCulture);
                else if (maxValue is Week)
                {
                    var date = ((Week)maxValue).StartDate();
                    maxValue = ((Week)maxValue).ToString() + "|" + date.Month + "|" + date.Day;
                }
                else if (maxValue is IConvertible) maxValue = (maxValue as IConvertible).ToString(CultureInfo.InvariantCulture);
            }
            ValidationParameters[MinValidationParameter] = minValue;
            ValidationParameters[MaxValidationParameter] = maxValue;
        }
    }
}
