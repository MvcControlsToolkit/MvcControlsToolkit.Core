using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using System.Globalization;


namespace MvcControlsToolkit.Core.Validation
{
    public class ModelClientValidationDRangeRule: ModelClientValidationRule
    {
        private const string DynamicRangeValidationType = "drange";
        private const string MinsValidationParameter = "dmins";
        private const string MaxsValidationParameter = "dmaxs";
        private const string MinDelaysValidationParameter = "dminds";
        private const string MaxDelaysValidationParameter = "dmaxds";

        public ModelClientValidationDRangeRule(string errorMessage, List<string> mins, List<string> minds, List<string> maxs, List<string> maxds)
            : base(DynamicRangeValidationType, errorMessage)
        {
            ValidationParameters[MinsValidationParameter] = mins == null? string.Empty : String.Join(" ", mins);
            ValidationParameters[MinDelaysValidationParameter] = minds == null ? string.Empty : String.Join(" ", minds);
            ValidationParameters[MaxsValidationParameter] = maxs == null ? string.Empty : String.Join(" ", maxs);
            ValidationParameters[MaxDelaysValidationParameter] = maxds == null ? string.Empty : String.Join(" ", maxds);
        }
    }
}
