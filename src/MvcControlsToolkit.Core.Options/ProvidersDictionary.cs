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
            foreach(var x in allProviders)
            {
                if (x.Key.StartsWith(prefix)){
                    foreach (var y in x.Value)
                    {
                        if (!requestProviders.Contains(y))
                        {
                            y.Load(context, dict);
                        }
                    }
                }
                
            }
        }
    }
}
