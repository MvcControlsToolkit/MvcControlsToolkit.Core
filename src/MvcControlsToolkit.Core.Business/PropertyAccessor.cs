using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.Business;
using System.ComponentModel;

namespace MvcControlsToolkit.Core
{
    public class PropertyAccessor
    {
        private object destination;
        private PropertyInfo property;
        private PropertyInfo metaProperty;
        private bool metaDataOnly = false;
        private bool createWhenNeeded = true;
        public object Value
        {
            set
            {
                if (metaDataOnly) return;
                property.SetValue(destination, value, new object[0]);
            }
            get
            {
                if (metaDataOnly) return null;
                return property.GetValue(destination, new object[0]);
            }
        }
        public PropertyInfo Property
        {
            get
            {
                return property;
            }
        }
        public object[] this[Type attributetType]
        {
            get
            {
                if (property == null) return null;
                object[] res0 = property.GetCustomAttributes(attributetType, true).ToArray();
                if (metaProperty == null) return res0;
                object[] res1 = metaProperty.GetCustomAttributes(attributetType, true).ToArray();

                if (res1 == null || res1.Length == 0) return res0;
                else return res1;


            }
        }
        protected DisplayAttribute Display
        {
            get
            {
                object[] displays = this[typeof(DisplayAttribute)];
                if (displays == null || displays.Length == 0) return null;
                else return displays[0] as DisplayAttribute;
            }
        }
        public string DisplayName
        {
            get
            {
                DisplayAttribute display = Display;
                if (display == null || string.IsNullOrWhiteSpace(display.Name)) return property.Name;
                else return display.Name;
            }
        }
        public PropertyAccessor(string expression, Type type)
        {
            metaDataOnly = true;
            createWhenNeeded = false;
            Initializer(null, expression, type);
        }
        public PropertyAccessor(object destination, string expression, bool createWhenNeeded = true)
        {
            this.createWhenNeeded = createWhenNeeded;
            Initializer(destination, expression, null);
        }
        protected void Initializer(object destination, string expression, Type type)
        {

            if (string.IsNullOrWhiteSpace(expression)) throw (new ArgumentNullException(expression));
            if (!metaDataOnly && expression.Contains("[")) throw new NotSupportedException(string.Format(Resources.NotSupportedEnumerables, nameof(PropertyAccessor)));

            Type currType;
            if (metaDataOnly)
                currType = type;
            else
                currType = destination.GetType();
            PropertyInfo currProperty = null;
            int index = 0;
            string[] fields = expression.Split(new string[] { "." }, StringSplitOptions.None);
            bool locked = false;
            foreach (string roughField in fields)
            {
                string field = roughField;
                if (metaDataOnly && field.IndexOf('[') >= 0) field = field.Substring(0, field.IndexOf('['));
                currProperty = currType.GetProperty(field);
                if (currProperty == null)
                {
                    if (locked || index >= fields.Length - 1)
                        throw new MvcControlsToolkit.Core.Exceptions.PropertyNotFoundException(field, currType);
                    else
                    {
                        index++;
                        continue;
                    }
                }
                else locked = true;
                if (index < fields.Length - 1)
                {
                    Type newType = currProperty.PropertyType;
                    if (!metaDataOnly)
                    {
                        object newValue = currProperty.GetValue(destination, new object[0]);
                        if (newValue == null)
                        {
                            if (createWhenNeeded)
                            {
                                if (newType.GetTypeInfo().IsClass)
                                {
                                    ConstructorInfo ci = newType.GetConstructor(new Type[0]);
                                    if (ci == null) throw new NotSupportedException(string.Format(Resources.NoConstructor, newType.Name));
                                    object newDestination = ci.Invoke(new object[0]);

                                    currProperty.SetValue(destination, newDestination, new object[0]);
                                    destination = newDestination;
                                    currType = newType;

                                }
                                else if (newType.GetTypeInfo().IsInterface) throw new NotSupportedException(string.Format(Resources.NotSupportedInterface, nameof(PropertyAccessor)));
                            }
                            else
                            {
                                destination = null;
                                currType = newType;
                                metaDataOnly = true;
                            }
                        }
                        else
                        {
                            currType = newType;
                            destination = newValue;
                        }
                    }
                    else
                        currType = newType;
                }
                else
                {
                    this.destination = destination;
                    property = currProperty;
#if net451
                    object[] metas = currProperty.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true);
                    MetadataTypeAttribute meta = null;
                    if (metas != null && metas.Length > 0) meta = metas[0] as MetadataTypeAttribute;
                    if (meta != null)
                        metaProperty = meta.MetadataClassType.GetProperty(property.Name);
#endif
                    break;
                }
                index++;
            }
        }
    }
}
