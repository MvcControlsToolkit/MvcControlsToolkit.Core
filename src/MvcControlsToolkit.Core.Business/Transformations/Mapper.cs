using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Transformations
{
    public class Mapper<S>
    {
        private MappingContext context;
        private S source;
        public Mapper(S source, MappingContext context=null)
        {
            if (context == null) context = MappingContext.Default;
            this.context = context;
            this.source = source;
        }
        public D To<D>()
        {
            if (source == null) return default(D);
            return context.Map<S, D>(source);
        }
    }
    public class IEnumerableMapper<S>
    {
        private MappingContext context;
        private IEnumerable<S> sources;
        public IEnumerableMapper(IEnumerable<S> sources, MappingContext context = null)
        {
            if (context == null) context = MappingContext.Default;
            this.context = context;
            this.sources = sources;
        }
        public IEnumerable<D> To<D>()
        {
            if (sources == null) return null;
            return context.MapIEnumerable<S, D>(sources);
        }
    }
}
