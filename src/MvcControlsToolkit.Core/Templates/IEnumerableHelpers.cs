using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public class CollectionInfo<T>
    {
        public Type ElementType { get; set; }
        public T Proto { get; set; }
    }
    public static class IEnumerableHelpers
    {
        public static T Element<T>(this IEnumerable<T> x)
        {
            return default(T);
        }
        public static CollectionInfo<T> Info<T>(this IEnumerable<T> x)
        {
            return new CollectionInfo<T>
            {
                ElementType = typeof(T)
            };
        }
        public static T SubElement<T>(this IEnumerable x)
        {
            return default(T);
        }
        public static CollectionInfo<T> SubInfo<T>(this IEnumerable x)
        {
            return new CollectionInfo<T>
            {
                ElementType = typeof(T)
            };
        }

    }
}
