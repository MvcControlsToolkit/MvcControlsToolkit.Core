using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MvcControlsToolkit.Core.Options
{
    internal class OptionObjectsDictionary: ConcurrentDictionary<Type, string>
    {
        public T Bind<T>(IOptionsDictionary dict, ProvidersDictionary prov, HttpContext ctx)
            where T : class, new()
        {
            string prefix = null;
            if(this.TryGetValue(typeof(T), out prefix))
            {
                prov.AddToRequest(prefix, ctx, dict);
                return (T)dict.GetOptionObject(prefix, typeof(T), new T());
            }
            return null;
        }
        public void Save<T> (T options, IOptionsDictionary dict, HttpContext ctx)
        {
            string prefix = null;
            HashSet<IOptionsProvider> set = null;
            if(this.TryGetValue(typeof(T), out prefix))
            {
                set= new HashSet<IOptionsProvider>(dict.AddOptionObject(null, prefix, uint.MaxValue));
                foreach(var x in set)
                {
                    if (x.Enabled(ctx) && x.CanSave) x.Save(ctx, dict);
                }
            }
        }
    }
}
