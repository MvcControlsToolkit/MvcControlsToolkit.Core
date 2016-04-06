using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcControlsToolkit.Core.Options.Providers
{
    public class RequestJsonProvider : IOptionsProvider
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
        public Func<HttpContext, bool> WhenEnabled { get; set; }
        public string Prefix { get; set; }

        public bool AutoSave { get; set; }

        public bool AutoCreate { get; set; }

        public string SourcePrefix { get; set; }
        
        public string FieldName { get; set; }

        public uint Priority { get; set; }

        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            var emptyPrefix = String.IsNullOrEmpty(SourcePrefix);
            
            string value =null;    
            if(ctx.Request.HasFormContentType) value = ctx.Request.Form[FieldName];
            if (value == null) value = ctx.Request.Query[FieldName];
            var res = new List<IOptionsProvider>();
            if (string.IsNullOrEmpty(value)) return res;
            var toAdd = (JsonConvert.DeserializeObject(value, typeof(List<KeyValuePair<string, string>>)) as List<KeyValuePair<string, string>>)
                .Where(m => emptyPrefix
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
            return res;
        }

        virtual public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            throw new NotImplementedException();
        }
        public RequestJsonProvider(string prefix, string fieldName)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName");
            Prefix = prefix;
            FieldName = fieldName;
        }
    }
}
