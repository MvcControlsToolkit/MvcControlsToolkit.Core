using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;

namespace MvcControlsToolkit.Core.Validation
{
    public class ValidationMetadataProvider : IValidationMetadataProvider
    {
        public void GetValidationMetadata(ValidationMetadataProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var a in context.Attributes)
            {
                object attribute = a;

                if (a is DataTypeAttribute)
                {
                    var attr = a as DataTypeAttribute;
                    if (attr.DataType == DataType.Url || attr.DataType == DataType.ImageUrl)
                    {
                        var x = new UrlAttribute();
                        if (attr.ErrorMessageResourceType != null)
                        {
                            x.ErrorMessageResourceType = attr.ErrorMessageResourceType;
                            x.ErrorMessageResourceName = attr.ErrorMessageResourceName;
                        }
                        else if(attr.ErrorMessage != null) x.ErrorMessage = attr.ErrorMessage;
                        attribute = x;
                    }
                    else if (attr.DataType == DataType.EmailAddress)
                    {
                        var x = new EmailAddressAttribute();
                        if (attr.ErrorMessageResourceType != null)
                        {
                            x.ErrorMessageResourceType = attr.ErrorMessageResourceType;
                            x.ErrorMessageResourceName = attr.ErrorMessageResourceName;
                        }
                        else if (attr.ErrorMessage != null) x.ErrorMessage = attr.ErrorMessage;
                        attribute = x;
                    }
                    else if ((attr.CustomDataType ?? "").ToLowerInvariant() == "color")
                    {
                        var x = new RegularExpressionAttribute("^#[a-fA-F0-9]{6}$");
                        if (attr.ErrorMessageResourceType != null)
                        {
                            x.ErrorMessageResourceType = attr.ErrorMessageResourceType;
                            x.ErrorMessageResourceName = attr.ErrorMessageResourceName;
                        }
                        else if (attr.ErrorMessage != null) x.ErrorMessage = attr.ErrorMessage;
                        attribute = x;
                    }
                }
                // If another provider has already added this attribute, do not repeat it.
                // This will prevent attributes like RemoteAttribute (which implement ValidationAttribute and
                // IClientModelValidator) to be added to the ValidationMetadata twice.
                // This is to ensure we do not end up with duplication validation rules on the client side.
                if (!context.ValidationMetadata.ValidatorMetadata.Contains(attribute))
                {
                    context.ValidationMetadata.ValidatorMetadata.Add(attribute);
                }
            }
        }
    }
}
