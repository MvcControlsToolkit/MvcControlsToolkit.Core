using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace MvcControlsToolkit.Core.Options
{
    internal class ProvidersDictionary
    {
        private static IDictionary<string, List<IOptionsProvider>> allProviders = new Dictionary<string, List<IOptionsProvider>>();
        private HashSet<IOptionsProvider> requestProviders = new HashSet<IOptionsProvider>();
        public static void Add(IOptionsProvider provider)
        {
            string prefix = provider.Prefix;
            List<IOptionsProvider> entry = null;
            if (allProviders.TryGetValue(prefix, out entry)){
                entry.Add(provider);
            }
            else {
                var l = allProviders[prefix] = new List<IOptionsProvider>();
                l.Add(provider);
            }
        }
        public void AddToRequest(string prefix, HttpContext context, IOptionsDictionary dict)
        {
            HashSet<IOptionsProvider> set = new HashSet<IOptionsProvider>();
            foreach (var x in allProviders)
            {
                if (x.Key == prefix || (x.Key.StartsWith(prefix) && x.Key[prefix.Length] == '.')){
                    foreach (var y in x.Value)
                    {
                        if (y.Enabled(context) && !requestProviders.Contains(y))
                        {
                            set.UnionWith(y.Load(context, dict));
                            requestProviders.Add(y);
                        }
                    }
                }
            }
            foreach(var x in set)
            {
                if (x.Enabled(context) && x.CanSave && x.AutoSave) x.Save(context, dict);
            }
        }
    }
}
