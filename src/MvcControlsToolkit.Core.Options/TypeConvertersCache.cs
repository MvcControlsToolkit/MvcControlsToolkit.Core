using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MvcControlsToolkit.Core.Options
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public static class TypeConvertersCache
    {
        private static IDictionary<Type, Func<object, Type, string>> store = new Dictionary<Type, Func<object, Type, string>>();
        private static IDictionary<Type, Func<string, Type, object>> inverseStore = new Dictionary<Type, Func<string, Type, object>>();
        static TypeConvertersCache()
        {
        }
        public static Func<object, string> GetConverter(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            Func<object,Type,string> pres = null;
            if (!store.TryGetValue(type, out pres)){
                if (isConvertible(type)) pres = IConvertibleConverter;
                var converter = TypeDescriptor.GetConverter(type);
                if (converter == null || !converter.CanConvertTo(typeof(string))) pres = null;
                else
                {
                    pres = (x, y) => converter.ConvertTo(null, CultureInfo.InvariantCulture, x, typeof(string)) as string;
                }
                store[type] = pres;
                
            }
            if (pres == null) return null;
            return y => pres(y, type);
        }
        public static Func<string, object> GetInverseConverter(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            Func<string, Type, object> pres = null;
            if (!inverseStore.TryGetValue(type, out pres))
            {
                if (isConvertible(type)) pres = IConvertibleInverseConverter;
                var converter = TypeDescriptor.GetConverter(type);
                if (converter == null || !converter.CanConvertFrom(typeof(string))) pres = null;
                else
                {
                    pres = (x, y) => converter.ConvertFromString(null, CultureInfo.InvariantCulture, x);
                }
                inverseStore[type] = pres;
            }
            if (pres == null) return null;
            return y => pres(y, type);
        }
        private static bool isConvertible(Type type)
        {
            return (typeof(IConvertible).IsAssignableFrom(type));
            
        }
        private static object IConvertibleInverseConverter(string x, Type type)
        {
            if (x == null) return null;
            if (type == typeof(string)) return x;
            return Convert.ChangeType(x, type, CultureInfo.InvariantCulture);
        }
        private static string IConvertibleConverter(object x, Type type)
        {
            if (x == null) return null;
            if (type == typeof(string)) return x as string;
            return Convert.ToString(x, CultureInfo.InvariantCulture);
        }
    }
}
