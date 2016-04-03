using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ViewFeatures;
using System.Reflection;

namespace MvcControlsToolkit.Core.Options.Providers
{
    public class EntityFrameworkProvider<T, M>: IOptionsProvider
        where T : class
        where M : class, new()
    {
        public bool CanSave
        {
            get
            {
                return false;
            }
        }
        public bool Enabled(HttpContext ctx)
        {
            return ctx.User != null && ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated;
        }

        public uint Priority { get; set; }

        public string Prefix { get; set; }

        protected Func<T, M> UserOptionsField { get; set; }

        protected string OptionFieldName { get; set; }
        public Expression<Func<T, M>> UserOptionsExpression { get; set; }

        public bool SaveDbContext { get; set; }

        public bool RelogUserAfterSave { get; set; }

        public bool PersistentSignIn { get; set; }

        public string AuthenticationMethod { get; set; }

        public bool AutoSave { get; set; }

        public List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict)
        {
            var res = new List<IOptionsProvider>();
            UserManager<T> um = ctx.RequestServices.GetService(typeof(UserManager<T>)) as UserManager<T>;
            var aUserT = um.FindByNameAsync(ctx.User.Identity.Name);
            aUserT.Wait();
            var user = aUserT.Result;
            var options = UserOptionsField(user);
            if (options == null) return res;
            return dict.AddOptionObject(this, Prefix, options, Priority, 1);

        }
        public void Save(HttpContext ctx, IOptionsDictionary dict)
        {
            var res = new List<IOptionsProvider>();
            UserManager<T> um = ctx.RequestServices.GetService(typeof(UserManager<T>)) as UserManager<T>;
            var aUserT = um.FindByNameAsync(ctx.User.Identity.Name);
            aUserT.Wait();
            var user = aUserT.Result;
            var options = UserOptionsField(user);
            if (options == null)
            {
                options = new M();
            }
            typeof(T).GetProperty(OptionFieldName).SetValue(user, options);
            dict.GetOptionObject(Prefix, typeof(M), options);
            if(SaveDbContext || RelogUserAfterSave)
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
        public EntityFrameworkProvider(Expression<Func<T, M>> userOptionsExpression)
        {
            if (userOptionsExpression == null) throw new ArgumentNullException("userOptionsExpression");
            UserOptionsExpression = userOptionsExpression;
            UserOptionsField = userOptionsExpression.Compile();
            OptionFieldName = ExpressionHelper.GetExpressionText(userOptionsExpression);
        }
    }
}
