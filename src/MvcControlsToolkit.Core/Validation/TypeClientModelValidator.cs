using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using MvcControlsToolkit.Core.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MvcControlsToolkit.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Reflection;
using System.Globalization;

namespace MvcControlsToolkit.Core.Validation
{
    public class TypeClientModelValidator : IClientModelValidator
    {
        
        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            int typeCode;
            string message = GetErrorMessage(context.ModelMetadata, out typeCode);
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-correcttype", message);
            MergeAttribute(context.Attributes, "data-val-correcttype-type", typeCode.ToString(CultureInfo.InvariantCulture));
        }
        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }

            attributes.Add(key, value);
            return true;
        }
        public static List<KeyValuePair<string, string>> GetAttributes(ModelMetadata modelMetadata)
        {
            var res = new List<KeyValuePair<string, string>>();
            int typeCode;
            string message = GetErrorMessage(modelMetadata, out typeCode);
            res.Add(new KeyValuePair<string, string>("data-val", "true"));
            res.Add(new KeyValuePair<string, string>("data-val-correcttype", message));
            res.Add(new KeyValuePair<string, string>("data-val-correcttype-type", typeCode.ToString(CultureInfo.InvariantCulture)));
            return res;
        }
        public static string GetErrorMessage(ModelMetadata modelMetadata, out int typeCode)
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
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeWeek)), modelMetadata.GetDisplayName());
            }
            else if (type == typeof(Month))
            {
                typeCode = 8;
                return string.Format(GetResourceMessage(nameof(DefaultMessages.ClientFieldMustBeMonth)), modelMetadata.GetDisplayName());
            }
            typeCode = 0;
            return string.Empty;
        }

        private static string GetResourceMessage(string name)
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
