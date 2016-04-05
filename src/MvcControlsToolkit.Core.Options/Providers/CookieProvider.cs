using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;


namespace MvcControlsToolkit.Core.Options.Providers
{
    public class CookieProvider : IOptionsProvider
    {
        virtual public bool CanSave
        {
            get
            {
                return true;
            }
        }
        virtual public bool Enabled(HttpContext ctx)
        {
            return WhenEnabled == null || WhenEnabled(ctx);
        }
        public Func<HttpContext, bool> WhenEnabled { get; set; }
        public string Prefix { get; set; }

        public bool AutoSave { get; set; }

        public bool AutoCreate { get; set; }

        public string SourcePrefix { get; set; }

        public string CookieName { get; set; }

        public TimeSpan Duration { get; set; }

        public uint Priority { get; set; }

        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            var emptyPrefix = String.IsNullOrEmpty(SourcePrefix);
            var cookie = ctx.Request.Cookies[CookieName].FirstOrDefault();
            var res = new List<IOptionsProvider>();
            if (string.IsNullOrEmpty(cookie)) return res;
            cookie = WebUtility.UrlDecode(cookie);
            var toAdd = (JsonConvert.DeserializeObject(cookie, typeof(List<KeyValuePair<string, string>>)) as List<KeyValuePair<string, string>>)
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
            var serialized = WebUtility.UrlEncode(JsonConvert.SerializeObject(dict.GetEntries(Prefix, String.IsNullOrEmpty(SourcePrefix) ? null : SourcePrefix)));
            ctx.Response.Cookies.Append(CookieName, serialized, new CookieOptions
            {
                HttpOnly=false,
                Expires = DateTime.Now.Add(Duration)
            });
        }
        public CookieProvider(string prefix, string cookieName)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            if (string.IsNullOrEmpty(cookieName)) throw new ArgumentNullException("cookieName");
            Prefix = prefix;
            CookieName = cookieName;
            Duration = TimeSpan.FromDays(365);
        }
    }
}
