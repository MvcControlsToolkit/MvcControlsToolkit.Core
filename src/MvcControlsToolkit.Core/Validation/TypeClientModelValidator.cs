using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Mvc.DataAnnotations;
using MvcControlsToolkit.Core.Types;
using Microsoft.AspNet.Mvc.ModelBinding;
using MvcControlsToolkit.Core.Extensions;
using Microsoft.AspNet.Mvc.Localization;
using System.Reflection;

namespace MvcControlsToolkit.Core.Validation
{
    public class TypeClientModelValidator : IClientModelValidator
    {
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            int typeCode;
            string message = GetErrorMessage(context.ModelMetadata, out typeCode);
            return new[] { new TypeClientModelValidationRule(message, typeCode) };
        }
        private string GetErrorMessage(ModelMetadata modelMetadata, out int typeCode)
        {
            if (modelMetadata == null)
            {
                throw new ArgumentNullException(nameof(modelMetadata));
            }

            var type = modelMetadata.UnderlyingOrModelType;
            var dataType = (modelMetadata.DataTypeName ?? modelMetadata.TemplateHint)?.ToLowerInvariant();
            if(type == typeof(int) || type == typeof(long) || type == typeof(short))
            {
                typeCode = 2;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeInteger)), modelMetadata.GetDisplayName());
            }
            else if (type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort))
            {
                typeCode = 1;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBePositiveInteger)), modelMetadata.GetDisplayName());
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                typeCode = 3;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeNumber)), modelMetadata.GetDisplayName());
            }
            else if (type == typeof(DateTime))
            {
                if (dataType == "date")
                {
                    typeCode = 5;
                    return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeDate)), modelMetadata.GetDisplayName());
                }
                else if (dataType == "time")
                {
                    typeCode = 4;
                        return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeTime)), modelMetadata.GetDisplayName());
                }
                else
                {
                    typeCode = 6;
                    return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeDateTime)), modelMetadata.GetDisplayName());
                }

            }
            else if (type == typeof(TimeSpan))
            {
                typeCode = 4;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeTime)), modelMetadata.GetDisplayName());
            }
            else if (type == typeof(Week))
            {
                typeCode = 7;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeWeak)), modelMetadata.GetDisplayName());
            }
            else if (type == typeof(Month))
            {
                typeCode = 8;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeMonth)), modelMetadata.GetDisplayName());
            }
            typeCode = 0;
            return string.Empty;
        }

        private string GetResourceMessage(string name)
        {
            PropertyInfo property = null;
            if (MvcControlsToolkitOptions.Instance.CustomMessagesResourceType != null)
            {
                property = MvcControlsToolkitOptions.Instance.CustomMessagesResourceType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            if(property == null)
            {
                property = typeof(DefaultMessages).GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            return property.GetValue(null) as string;
        }

    }
}
