using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace MvcControlsToolkit.Core.Options.Providers
{
    public class RequestProvider : IOptionsProvider
    {
        virtual public bool CanSave
        {
            get
            {
                return false;
            }
        }
        virtual public bool Enabled(HttpContext ctx)
        {
            return true;
        }
        public string Prefix { get; set; }
        public bool AutoSave { get; set; }

        public string SourcePrefix { get; set; }

        public uint Priority { get; set; }

        public bool UseForm { get; set; }

        public bool UseParams { get; set; }

        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            var emptyPrefix = String.IsNullOrEmpty(SourcePrefix);
            var res = new List<IOptionsProvider>();
            if (UseForm)
            {
                var form = ctx.Request.Form;
                
                var toAdd = form.Where(m => emptyPrefix
                  || m.Key == SourcePrefix
                  || (m.Key.StartsWith(SourcePrefix) && m.Key[SourcePrefix.Length] == '.'))
                .Select(m => new
                {
                    Key = Prefix+"."+(emptyPrefix ? m.Key : m.Key.Substring(SourcePrefix.Length + 1)),
                    Value = m.Value
                });
                foreach(var x in toAdd)
                {
                    var pres=dict.AddOption(this, x.Key, x.Value, Priority);
                    if (pres != null) res.Add(pres);
                }

            }
            if (UseParams)
            {
                var pars = ctx.Request.Query;
                var toAdd=pars.Where(m => emptyPrefix
                  || m.Key == SourcePrefix
                  || (m.Key.StartsWith(SourcePrefix) && m.Key[SourcePrefix.Length] == '.'))
                .Select(m => new
                {
                    Key = Prefix + "." + (emptyPrefix ? m.Key : m.Key.Substring(SourcePrefix.Length + 1)),
                    Value = m.Value
                });
                foreach (var x in toAdd)
                {
                    var pres = dict.AddOption(this, x.Key, x.Value, Priority);
                    if (pres != null) res.Add(pres);
                }
            }
            return res;
        }

        virtual public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            throw new NotImplementedException();
        }

        public RequestProvider(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            Prefix = prefix;
            if (!UseForm && !UseParams)
            {
                UseParams = true;
                UseForm = true;
            }
        }
    }
}
