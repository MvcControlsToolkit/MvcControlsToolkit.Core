using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace MvcControlsToolkit.Core.ModelBinding
{
    internal class DiffMetaData: ModelMetadata, IEqualityComparer<ModelMetadata>
    {
        public DiffMetaData(ModelMetadata subClass, ModelMetadata superClass)
            :base(ModelMetadataIdentity.ForProperty(subClass.ModelType, "subclass", superClass.ModelType))
        {
            this.subClass = subClass;
            this.superClass = superClass;
        }
        private ModelMetadata superClass, subClass;
        private ModelPropertyCollection _properties;
        /// <summary>
        /// Gets the set of attributes for the current instance.
        /// </summary>



        /// <inheritdoc />
        public override IReadOnlyDictionary<object, object> AdditionalValues
        {
            get
            {
                return subClass.AdditionalValues;
            }
        }

        /// <inheritdoc />
        public override BindingSource BindingSource
        {
            get
            {
                return subClass.BindingSource;
            }
        }

        /// <inheritdoc />
        public override string BinderModelName
        {
            get
            {
                return subClass.BinderModelName;
            }
        }

        /// <inheritdoc />
        public override Type BinderType
        {
            get
            {
                return subClass.BinderType;
            }
        }

        /// <inheritdoc />
        public override bool ConvertEmptyStringToNull
        {
            get
            {
                return subClass.ConvertEmptyStringToNull;
            }
        }

        /// <inheritdoc />
        public override string DataTypeName
        {
            get
            {
                return subClass.DataTypeName;
            }
        }

        /// <inheritdoc />
        public override string Description
        {
            get
            {
                return subClass.Description;
            }
        }

        /// <inheritdoc />
        public override string DisplayFormatString
        {
            get
            {
                return subClass.DisplayFormatString;
            }
        }

        /// <inheritdoc />
        public override string DisplayName
        {
            get
            {
                return subClass.DisplayName;
            }
        }

        /// <inheritdoc />
        public override string EditFormatString
        {
            get
            {
                return subClass.EditFormatString;
            }
        }

        /// <inheritdoc />
        public override ModelMetadata ElementMetadata
        {
            get
            {
                return subClass.ElementMetadata;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues
        {
            get
            {
                return subClass.EnumGroupedDisplayNamesAndValues;
            }
        }

        /// <inheritdoc />
        public override IReadOnlyDictionary<string, string> EnumNamesAndValues
        {
            get
            {
                return subClass.EnumNamesAndValues;
            }
        }

        /// <inheritdoc />
        public override bool HasNonDefaultEditFormat
        {
            get
            {
                return subClass.HasNonDefaultEditFormat;
            }
        }

        /// <inheritdoc />
        public override bool HideSurroundingHtml
        {
            get
            {
                return subClass.HideSurroundingHtml;
            }
        }

        /// <inheritdoc />
        public override bool HtmlEncode
        {
            get
            {
                return subClass.HtmlEncode;
            }
        }

        /// <inheritdoc />
        public override bool IsBindingAllowed
        {
            get
            {
                return subClass.IsBindingAllowed;
            }
        }

        /// <inheritdoc />
        public override bool IsBindingRequired
        {
            get
            {
                return subClass.IsBindingRequired;
            }
        }

        /// <inheritdoc />
        public override bool IsEnum
        {
            get
            {
                return subClass.IsEnum;
            }
        }

        /// <inheritdoc />
        public override bool IsFlagsEnum
        {
            get
            {
                return subClass.IsFlagsEnum;
            }
        }

        /// <inheritdoc />
        public override bool IsReadOnly
        {
            get
            {
                return subClass.IsReadOnly;
            }
        }

        /// <inheritdoc />
        public override bool IsRequired
        {
            get
            {
                return subClass.IsRequired;
            }
        }

        /// <inheritdoc />
        public override IModelBindingMessageProvider ModelBindingMessageProvider
        {
            get
            {
                return subClass.ModelBindingMessageProvider;
            }
        }

        /// <inheritdoc />
        public override string NullDisplayText
        {
            get
            {
                return subClass.NullDisplayText;
            }
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return subClass.Order;
            }
        }

        /// <inheritdoc />
        public override string Placeholder
        {
            get
            {
                return subClass.Placeholder;
            }
        }

        /// <inheritdoc />
        public override ModelPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    var properties = subClass.Properties.Except(superClass.Properties, this);
                    
                    _properties = new ModelPropertyCollection(properties);
                }

                return _properties;
            }
        }

        /// <inheritdoc />
        public override IPropertyFilterProvider PropertyFilterProvider
        {
            get
            {
                return subClass.PropertyFilterProvider;
            }
        }

        /// <inheritdoc />
        public override bool ShowForDisplay
        {
            get
            {
                return subClass.ShowForDisplay;
            }
        }

        /// <inheritdoc />
        public override bool ShowForEdit
        {
            get
            {
                return subClass.ShowForEdit;
            }
        }

        /// <inheritdoc />
        public override string SimpleDisplayProperty
        {
            get
            {
                return subClass.SimpleDisplayProperty;
            }
        }

        /// <inheritdoc />
        public override string TemplateHint
        {
            get
            {
                return subClass.TemplateHint;
            }
        }

        /// <inheritdoc />
        public override bool ValidateChildren
        {
            get
            {
                return subClass.ValidateChildren;
            }
        }

        /// <inheritdoc />
        public override IReadOnlyList<object> ValidatorMetadata
        {
            get
            {
                return subClass.ValidatorMetadata;
            }
        }

        /// <inheritdoc />
        public override Func<object, object> PropertyGetter
        {
            get
            {
                return subClass.PropertyGetter;
            }
        }

        /// <inheritdoc />
        public override Action<object, object> PropertySetter
        {
            get
            {
                return subClass.PropertySetter;
            }
        }

        public bool Equals(ModelMetadata x, ModelMetadata y)
        {
            return x.PropertyName == y.PropertyName;
        }

        public int GetHashCode(ModelMetadata obj)
        {
            return obj.PropertyName == null ? 1 : obj.PropertyName.GetHashCode();
        }
    }
}
