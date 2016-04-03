using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace MvcControlsToolkit.Core.Options
{
    internal class OptionObjectsDictionary: Dictionary<Type, string>
    {
        public T Bind<T>(IOptionsDictionary dict)
            where T : class, new()
        {
            string prefix = null;
            if(this.TryGetValue(typeof(T), out prefix))
            {
                return (T)dict.GetOptionObject(prefix, typeof(T));
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
