using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations;


namespace MvcControlsToolkit.Core.Validation
{
    public class DataAnnotationsClientModelValidatorProviderExt : IClientModelValidatorProvider
    {
        internal delegate IClientModelValidator DataAnnotationsClientModelValidationFactory(
            ValidationAttribute attribute,
            IStringLocalizer stringLocalizer);

        private readonly Dictionary<Type, DataAnnotationsClientModelValidationFactory> _attributeFactories =
            BuildAttributeFactoriesDictionary();
        private readonly IOptions<MvcDataAnnotationsLocalizationOptions> _options;
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public DataAnnotationsClientModelValidatorProviderExt(
            IOptions<MvcDataAnnotationsLocalizationOptions> options,
            IStringLocalizerFactory stringLocalizerFactory)
        {
            _options = options;
            _stringLocalizerFactory = stringLocalizerFactory;
        }
        internal Dictionary<Type, DataAnnotationsClientModelValidationFactory> AttributeFactories
        {
            get { return _attributeFactories; }
        }

        public void GetValidators(ClientValidatorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            IStringLocalizer stringLocalizer = null;
            if (_options.Value.DataAnnotationLocalizerProvider != null && _stringLocalizerFactory != null)
            {
                // This will pass first non-null type (either containerType or modelType) to delegate.
                // Pass the root model type(container type) if it is non null, else pass the model type.
                stringLocalizer = _options.Value.DataAnnotationLocalizerProvider(
                    context.ModelMetadata.ContainerType ?? context.ModelMetadata.ModelType,
                    _stringLocalizerFactory);
            }

            var hasRequiredAttribute = false;

            foreach (var attribute in context.ValidatorMetadata.OfType<ValidationAttribute>())
            {
                hasRequiredAttribute |= attribute is RequiredAttribute;

                DataAnnotationsClientModelValidationFactory factory;
                if (_attributeFactories.TryGetValue(attribute.GetType(), out factory))
                {
                    context.Validators.Add(factory(attribute, stringLocalizer));
                }
            }

            if (!hasRequiredAttribute && context.ModelMetadata.IsRequired)
            {
                // Add a default '[Required]' validator for generating HTML if necessary.
                context.Validators.Add(new RequiredAttributeAdapter(new RequiredAttribute(), stringLocalizer));
            }
        }

        private static Dictionary<Type, DataAnnotationsClientModelValidationFactory> BuildAttributeFactoriesDictionary()
        {
            return new Dictionary<Type, DataAnnotationsClientModelValidationFactory>()
            {
                {
                    typeof(RegularExpressionAttribute),
                    (attribute, stringLocalizer) => new RegularExpressionAttributeAdapter(
                        (RegularExpressionAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(MaxLengthAttribute),
                    (attribute, stringLocalizer) => new MaxLengthAttributeAdapter(
                        (MaxLengthAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(MinLengthAttribute),
                    (attribute, stringLocalizer) => new MinLengthAttributeAdapter(
                        (MinLengthAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(CompareAttribute),
                    (attribute, stringLocalizer) => new CompareAttributeAdapter(
                        (CompareAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(RequiredAttribute),
                    (attribute, stringLocalizer) => new RequiredAttributeAdapter(
                        (RequiredAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(RangeAttribute),
                    (attribute, stringLocalizer) => new RangeAttributeAdapterExt(
                        (RangeAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(DynamicRangeAttribute),
                    (attribute, stringLocalizer) => new DynamicRangeAttributeAdapter(
                        (DynamicRangeAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(StringLengthAttribute),
                    (attribute, stringLocalizer) => new StringLengthAttributeAdapter(
                        (StringLengthAttribute)attribute,
                        stringLocalizer)
                },
                {
                    typeof(CreditCardAttribute),
                    (attribute, stringLocalizer) => new DataTypeAttributeAdapter(
                        (DataTypeAttribute)attribute,
                        "creditcard",
                        stringLocalizer)
                },
                {
                    typeof(EmailAddressAttribute),
                    (attribute, stringLocalizer) => new DataTypeAttributeAdapter(
                        (DataTypeAttribute)attribute,
                        "email",
                        stringLocalizer)
                },
                {
                    typeof(PhoneAttribute),
                    (attribute, stringLocalizer) => new DataTypeAttributeAdapter(
                        (DataTypeAttribute)attribute,
                        "phone",
                        stringLocalizer)
                },
                {
                    typeof(UrlAttribute),
                    (attribute, stringLocalizer) => new DataTypeAttributeAdapter(
                        (DataTypeAttribute)attribute,
                        "url",
                        stringLocalizer)
                }
            };
        }
    }
}
