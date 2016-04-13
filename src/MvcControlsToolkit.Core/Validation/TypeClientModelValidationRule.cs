using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;

namespace MvcControlsToolkit.Core.Validation
{
    public class TypeClientModelValidationRule: ModelClientValidationRule
    {
        private const string correctTypeValidationType = "correcttype";
        private const string correctTypeValidationParameter = "type";

        public TypeClientModelValidationRule(
            string errorMessage,
            int typeCode)
            : base(correctTypeValidationType, errorMessage)
        {
            ValidationParameters[correctTypeValidationParameter] = typeCode;
        }   
    }
}
