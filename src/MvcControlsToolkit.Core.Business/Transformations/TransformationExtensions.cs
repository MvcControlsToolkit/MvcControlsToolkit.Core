using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Transformations
{
    public static class TransformationExtensions
    {
        public static Mapper<S> Map<S>(this S source, MappingContext context=null)
        {
            return new Mapper<S>(source, context);
        }
        public static IEnumerableMapper<S> MapIEnumerable<S>(this IEnumerable<S> sources, MappingContext context = null)
        {
            return new IEnumerableMapper<S>(sources, context);
        }
    }
}
