using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.ITests
{
    public class IEnumerableToArray<T> : IBindingTransformation<IEnumerable<T>, T[], List<T>>
    {
        public List<T> InverseTransform(T[] x)
        {
            if (x == null) return new List<T>();
            return x.ToList();
        }

        public T[] Transform(IEnumerable<T> x)
        {
            if (x == null) return new T[0];
            return x.ToArray();
        }
    }
}
