using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MvcControlsToolkit.Core.Options.Providers
{
    public class ODataQueryOptionsProvider:IOptionsProvider
    {
        private static string[] clauses = new string[]
        {
            "$filter",
            "$apply",
            "$orderby",
            "$search",
            "$top",
            "$skip"
        };
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

        public bool AutoCreate { get; set; }

        

        public uint Priority { get; set; }

        public bool UseForm { get; set; }

        public bool UseParams { get; set; }

        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            
            var res = new List<IOptionsProvider>();
            StringValues value;
            if (UseForm && ctx.Request.HasFormContentType)
            {
                var form = ctx.Request.Form;

                foreach (var x in clauses)
                {

                    if (form.TryGetValue(x, out value))
                    {
                        var pres = dict.AddOption(this, Prefix + "." + x, value.ToString(), Priority+1);
                        if (pres != null) res.Add(pres);
                    }

                }

            }
            if (UseParams)
            {
                var pars = ctx.Request.Query;
                
                foreach (var x in clauses)
                {
                    
                    if(pars.TryGetValue(x, out value))
                    {
                        var pres = dict.AddOption(this, Prefix+"."+x, value.ToString(), Priority);
                        if (pres != null) res.Add(pres);
                    }
                    
                }
            }
            return res;
        }

        virtual public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            throw new NotImplementedException();
        }

        public ODataQueryOptionsProvider(string prefix)
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
