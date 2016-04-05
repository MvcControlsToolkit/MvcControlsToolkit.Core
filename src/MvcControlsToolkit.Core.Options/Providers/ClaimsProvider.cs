using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;

namespace MvcControlsToolkit.Core.Options.Providers
{
    public class ClaimsProvider<T>: IOptionsProvider
        where T: class
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
            return ctx.User != null && ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated;
        }
        public uint Priority { get; set; }
        
        public string Prefix { get; set; }

        public bool PersistentSignIn { get; set; }

        public string AuthenticationMethod { get; set; }

        public bool AutoSave { get; set; }

        public bool AutoCreate { get; set; }
        public string SourcePrefix { get; set; }

        virtual public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            
            var emptyPrefix = String.IsNullOrEmpty(SourcePrefix);
            var toAdd = ctx.User.Claims.Where(m => m.Type == SourcePrefix || (m.Type.StartsWith(SourcePrefix) && m.Type[SourcePrefix.Length] == '/'))
                .Select(m => new
                {
                    Key = m.Type,
                    Value = m
                });
            
            var comparer = new Dictionary<string, System.Security.Claims.Claim>();
            foreach (var x in toAdd)
            {
                comparer.Add(x.Key, x.Value);
            }

            var changes=dict.GetEntries(Prefix);
            List<System.Security.Claims.Claim> ToAddC = new List<System.Security.Claims.Claim>();
            List<System.Security.Claims.Claim> ToRemove = new List<System.Security.Claims.Claim>();
            foreach(var c in changes)
            {
                var key = emptyPrefix ? c.Key.Replace(".", "/") : SourcePrefix+"/"+ c.Key.Replace(".", "/");
                var val = c.Value;
                System.Security.Claims.Claim prev = null;
                if (comparer.TryGetValue(key, out prev))
                {
                    ToAddC.Add(new System.Security.Claims.Claim(prev.Type, val, prev.ValueType, prev.Issuer, prev.OriginalIssuer, prev.Subject));
                    ToRemove.Add(prev);
                }
                else
                    ToAddC.Add(new System.Security.Claims.Claim(key, val));
            }
            if (changes.Count > 0)
            {
                UserManager<T> um = ctx.RequestServices.GetService(typeof(UserManager<T>)) as UserManager<T>;
                SignInManager<T> sm = ctx.RequestServices.GetService(typeof(SignInManager<T>)) as SignInManager<T>;
                var aUserT = um.FindByNameAsync(ctx.User.Identity.Name);
                aUserT.Wait();
                if (ToRemove.Count > 0)
                {
                    var t = um.RemoveClaimsAsync(aUserT.Result, ToRemove);
                    t.Wait();
                }
                if (ToAddC.Count > 0)
                {
                    var t = um.AddClaimsAsync(aUserT.Result, ToAddC);
                    t.Wait();
                }
                var to = sm.SignOutAsync();
                to.Wait();
                var tl = sm.SignInAsync(aUserT.Result, PersistentSignIn, AuthenticationMethod);
                tl.Wait();
                
            }
        }
        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            var res = new List<IOptionsProvider>();
            var emptyPrefix = String.IsNullOrEmpty(SourcePrefix);
            var toAdd = ctx.User.Claims.Where(m => m.Type == SourcePrefix || (m.Type.StartsWith(SourcePrefix) && m.Type[SourcePrefix.Length] == '/'))
                .Select(m => new
                {
                    Key = emptyPrefix ? Prefix + "." + m.Type.Replace("/", ".") : Prefix + "." + m.Type.Substring(SourcePrefix.Length + 1).Replace("/", "."),
                    Value = m.Value
                });
            foreach (var x in toAdd)
            {
                var pres = dict.AddOption(this, x.Key, x.Value, Priority);
                if (pres != null) res.Add(pres);
            }
            return res;
        }
        
        public ClaimsProvider(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException("prefix");
            
            Prefix = prefix;
            
        }
    }
}
