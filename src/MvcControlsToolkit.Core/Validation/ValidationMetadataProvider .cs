using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MvcControlsToolkit.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Reflection;

namespace MvcControlsToolkit.Core.Validation
{
    public class ValidationMetadataProvider : IValidationMetadataProvider
    {
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            Type resourceType = null;
            
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (MvcControlsToolkitOptions.Instance.CustomMessagesResourceType != null)
            {
                resourceType = MvcControlsToolkitOptions.Instance.CustomMessagesResourceType;
                
            }
            
            foreach (var a in context.Attributes)
            {

                object attribute = a;
                if (attribute is DataTypeAttribute)
                {
                    
                        var attr = a as DataTypeAttribute;
                        if (attr.DataType == DataType.Url || attr.DataType == DataType.ImageUrl)
                        {
                            var x = new UrlAttribute();
                            
                            if (attr.ErrorMessageResourceName != null)
                            {
                                x.ErrorMessageResourceType = attr.ErrorMessageResourceType;
                                x.ErrorMessageResourceName = attr.ErrorMessageResourceName;
                            }
                            else if(resourceType != null && resourceType.GetProperty("UrlAttribute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) != null)
                            {
                                x.ErrorMessageResourceType = resourceType;
                                x.ErrorMessageResourceName = "UrlAttribute";
                            }
                            else
                            {
                                x.ErrorMessageResourceType = typeof(DefaultMessages);
                                x.ErrorMessageResourceName = "UrlAttribute";
                            }
                            attribute = x;
                        }
                        else if (attr.DataType == DataType.EmailAddress)
                        {
                            var x = new EmailAddressAttribute();
                            
                            if (attr.ErrorMessageResourceName != null)
                            {
                                x.ErrorMessageResourceType = attr.ErrorMessageResourceType;
                                x.ErrorMessageResourceName = attr.ErrorMessageResourceName;
                            }
                            else if (resourceType != null && resourceType.GetProperty("EmailAddressAttribute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) != null)
                            {
                                x.ErrorMessageResourceType = resourceType;
                                x.ErrorMessageResourceName = "EmailAddressAttribute";
                            }
                            else
                            {
                                x.ErrorMessageResourceType = typeof(DefaultMessages);
                                x.ErrorMessageResourceName = "EmailAddressAttribute";
                            }
                        attribute = x;
                        }
                        else if ((attr.CustomDataType ?? "").ToLowerInvariant() == "color")
                        {
                            var x = new RegularExpressionAttribute("^#[a-fA-F0-9]{6}$");

                            if (attr.ErrorMessageResourceName != null)
                            {
                                x.ErrorMessageResourceType = attr.ErrorMessageResourceType;
                                x.ErrorMessageResourceName = attr.ErrorMessageResourceName;
                                
                            }
                            else if (resourceType != null && resourceType.GetProperty("ColorAttribute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) != null)
                            {
                                x.ErrorMessageResourceType = resourceType;
                                x.ErrorMessageResourceName = "ColorAttribute";
                            }
                            else
                            {
                                x.ErrorMessageResourceType = typeof(DefaultMessages);
                                x.ErrorMessageResourceName = "ColorAttribute";
                            }
                        attribute = x;
                        }
                    
                    
                    
                    if (attribute != a && (context.ValidationMetadata.ValidatorMetadata.Where(m => m.GetType() == attribute.GetType()).SingleOrDefault()==null))
                    {
                        context.ValidationMetadata.ValidatorMetadata.Add(attribute);
                    }
                }
            }
            foreach(var attribute in context.ValidationMetadata.ValidatorMetadata)
            {
                ValidationAttribute tAttr = attribute as ValidationAttribute;
                if (tAttr != null && resourceType != null && tAttr.ErrorMessage == null && tAttr.ErrorMessageResourceName == null)
                {
                    var name = tAttr.GetType().Name;
                    if (resourceType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) != null)
                    {
                        tAttr.ErrorMessageResourceType = resourceType;
                        tAttr.ErrorMessageResourceName = name;
                        tAttr.ErrorMessage = null;
                    }
                }
            }
        }
    }
}
