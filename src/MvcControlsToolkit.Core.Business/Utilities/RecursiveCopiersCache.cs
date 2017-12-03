using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities.Internal
{
    public static class RecursiveCopiersCache
    {
        private static ConcurrentDictionary<Tuple<Type, Type>, object> allStronglyTypedCopiers
            = new ConcurrentDictionary<Tuple<Type, Type>, object>();
        //private static ConcurrentDictionary<Tuple<Type, Type>, IObjectCopier> allCopiers
        //    = new ConcurrentDictionary<Tuple<Type, Type>, IObjectCopier>();
        //public static bool DeclareCopierSpecifications(Type sourceType, Type DestinationType, LambdaExpression expression)
        //{
        //    return allCopiers.TryAdd(Tuple.Create(sourceType, DestinationType), new RecursiveObjectCopier(expression));
        //}
        public static RecursiveObjectCopier<TSource, TDest> DeclareCopierSpecifications<TSource, TDest>(Expression<Func<TSource, TDest>> expression)
            where TDest: class, new()
        {
            var copier = new RecursiveObjectCopier<TSource, TDest>(expression);
            if (allStronglyTypedCopiers.TryAdd(Tuple.Create(typeof(TSource), typeof(TDest)), copier))
                return copier;
            else return Get<TSource, TDest>() as RecursiveObjectCopier<TSource, TDest>;
        }
        //public static IObjectCopier Get(Type sourceType, Type DestinationType)
        //{
        //    IObjectCopier res;
        //    if (allCopiers.TryGetValue(Tuple.Create(sourceType, DestinationType), out res)) return res;
        //    else return null;
        //}
        public static IObjectCopier<TSource, TDest> Get<TSource, TDest>()
        {
            object res;
            if (allStronglyTypedCopiers.TryGetValue(Tuple.Create(typeof(TSource), typeof(TDest)), out res)) return res as IObjectCopier<TSource, TDest>;
            else return null;
        }
        public static IObjectCopier<TSource, TDest> GetOrDefault<TSource, TDest>()
            where TDest : class, new()
        {
            var res = Get<TSource, TDest>();
            if (res == null)
                return DeclareCopierSpecifications<TSource, TDest>(null) as IObjectCopier<TSource, TDest>;
            else return res;
        }
    }
}
