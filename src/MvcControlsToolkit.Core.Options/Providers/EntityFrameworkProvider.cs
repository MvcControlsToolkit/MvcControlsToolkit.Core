using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace MvcControlsToolkit.Core.Options.Providers
{
    public class EntityFrameworkProvider<T, M>: IOptionsProvider
        where T : class
        where M : class, new()
    {
        virtual public bool CanSave
        {
            get
            {
                return true;
            }
        }
        public bool Enabled(HttpContext ctx)
        {
            return ctx.User != null && ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated;
        }

        public uint Priority { get; set; }

        public string Prefix { get; set; }

        protected Func<T, M> UserOptions { get; set; }

        public Action<T, M> ApplyOptionsToUser { get; set; }

        protected string OptionFieldName { get; set; }
        public Expression<Func<T, M>> UserOptionsExpression { get; set; }

        public bool SaveDbContext { get; set; }

        public bool RelogUserAfterSave { get; set; }

        public bool PersistentSignIn { get; set; }

        public string AuthenticationMethod { get; set; }

        public bool AutoSave { get; set; }

        public bool AutoCreate { get; set; }

        virtual public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            var res = new List<IOptionsProvider>();
            UserManager<T> um = ctx.RequestServices.GetService(typeof(UserManager<T>)) as UserManager<T>;
            var aUserT = um.FindByNameAsync(ctx.User.Identity.Name);
            aUserT.Wait();
            var user = aUserT.Result;
            var options = UserOptions(user);
            if (options == null) return res;
            return dict.AddOptionObject(this, Prefix, options, Priority, 1);

        }
        virtual public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            var res = new List<IOptionsProvider>();
            UserManager<T> um = ctx.RequestServices.GetService(typeof(UserManager<T>)) as UserManager<T>;
            var aUserT = um.FindByNameAsync(ctx.User.Identity.Name);
            aUserT.Wait();
            var user = aUserT.Result;
            if (ApplyOptionsToUser == null)
            {
                var options = UserOptions(user);
                if (options == null)
                {
                    options = new M();
                }
                typeof(T).GetProperty(OptionFieldName).SetValue(user, options);
                dict.GetOptionObject(Prefix, typeof(M), options);
            }
            else {
                var options = dict.GetOptionObject(Prefix, typeof(M)) as M;
                ApplyOptionsToUser(user, options);
            }
                
            if (SaveDbContext || RelogUserAfterSave)
            {
                var aw = um.UpdateAsync(user);
                aw.Wait();
                if (RelogUserAfterSave)
                {
                    SignInManager<T> sm = ctx.RequestServices.GetService(typeof(SignInManager<T>)) as SignInManager<T>;
                    var to = sm.SignOutAsync();
                    to.Wait();
                    var tl = sm.SignInAsync(aUserT.Result, PersistentSignIn, AuthenticationMethod);
                    tl.Wait();
                }
            }
        }
        public EntityFrameworkProvider(string prefix, Expression<Func<T, M>> userOptionsExpression)
        {
            if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentNullException("prefix");
            Prefix = prefix;
            if (userOptionsExpression == null) throw new ArgumentNullException("userOptionsExpression");
            UserOptionsExpression = userOptionsExpression;
            UserOptions = userOptionsExpression.Compile();
            try {
                OptionFieldName = ExpressionHelper.GetExpressionText(userOptionsExpression);
            }
            catch { }
        }
    }
}
