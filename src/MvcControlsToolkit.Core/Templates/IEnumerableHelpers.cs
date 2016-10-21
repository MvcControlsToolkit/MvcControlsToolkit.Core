using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public class TypeProto<T>
    {
        public Type ElementType { get; set; }
        public T Model { get; set; }
    }
    
    public static class IEnumerableHelpers
    {

        public static string CleanPrefix(this string x)
        {
            if (string.IsNullOrEmpty(x)) return x;
            return string.Join(".", x.Split('.').Where(m => !Char.IsDigit(m[0]) ));
        }

        public static T Element<T>(this IEnumerable<T> x)
        {
            return default(T);
        }
        public static TypeProto<T> Info<T>(this IEnumerable<T> x)
        {
            return new TypeProto<T>
            {
                ElementType = typeof(T)
            };
        }
        public static T SubElement<T>(this object x)
            where T : class
        {
            return x as T;
        }
        public static TypeProto<T> SubInfo<T>(this object x)
            where T: class
        {
            return new TypeProto<T>
            {
                ElementType = typeof(T),
                Model=x as T
            };
        }
        public static T SubElement<T>(this IEnumerable x)
        {
            return default(T);
        }
        public static TypeProto<T> SubInfo<T>(this IEnumerable x)
        {
            return new TypeProto<T>
            {
                ElementType = typeof(T)
            };
        }

    }
}
