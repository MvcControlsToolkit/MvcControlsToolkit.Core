using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Utilities;

namespace MvcControlsToolkit.Core.Business.Transformations
{

    
    public class MappingContext
    {
        private ConcurrentDictionary<Tuple<Type, Type>, object> mappings =
        new ConcurrentDictionary<Tuple<Type, Type>, object>();
        public static MappingContext Default { get; private set; }
        = new MappingContext();

public MappingContext Add<S, D>(
    Expression<Func<S, D>> expression =null)
    where D: class, new()
        {
            var pair = Tuple.Create(typeof(S), typeof(D));
            if (!mappings.ContainsKey(pair))
                mappings.TryAdd(pair, new RecursiveObjectCopier<S, D>(expression));
            return this;
        }
        public D Map<S, D>(S o)
        {
            if (o == null) return default(D);
            var pair = Tuple.Create(typeof(S), typeof(D));
            object res = null;
            if(!mappings.TryGetValue(pair, out res))
            {
                res = Activator.CreateInstance(typeof(RecursiveObjectCopier<,>).MakeGenericType(typeof(S), typeof(D)), new object[] {null });
                mappings.TryAdd(pair, res);
            }
            return (res as IObjectCopier<D>).Copy(o, default(D));
        }
        
        public IEnumerable<D> MapIEnumerable<S, D>(IEnumerable<S> sources)
        {

            var pair = Tuple.Create(typeof(S), typeof(D));
            object res = null;
            if (!mappings.TryGetValue(pair, out res))
            {
                res = Activator.CreateInstance(typeof(RecursiveObjectCopier<,>).MakeGenericType(typeof(S), typeof(D)), new object[] { null });
                mappings.TryAdd(pair, res);
            }
            var copier = res as IObjectCopier<D>;
            return sources.Select(m => copier.Copy(m, default(D)));
        }
    }
}
