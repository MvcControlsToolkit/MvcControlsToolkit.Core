using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;

namespace MvcControlsToolkit.Core.Views
{
    public static class ScopeExtension
    {
        private static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        public static RenderingScope<T> Scope<T>(this IHtmlHelper h, string newPrefix, T model = default(T))
        {
            if (newPrefix == null) newPrefix = string.Empty;
            return new RenderingScope<T>(model, h.ViewData.GetFullHtmlFieldName(newPrefix), h.ViewData);
        }
        public static RenderingScope<T> Scope<T>(this IHtmlHelper h, T model = default(T))
        {
            return new RenderingScope<T>(model, h.ViewData.TemplateInfo.HtmlFieldPrefix, h.ViewData);
        }
        public static RenderingScope<T> Scope<M, T>(this IHtmlHelper<M> h, Expression<Func<M, T>> expression, string newPrefix = "")
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            T model = default(T);
            try
            {
                model = expression.Compile().Invoke(h.ViewData.Model);
            }
            catch { }
            return Scope<T>(h, newPrefix, model);
        }
        public static RenderingScope<I> Transform<M, S, I, D>(this IHtmlHelper<M> h, Expression<Func<M, S>> expression, IBindingTransformation<S,I, D> transform)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            transform.Context = h.ViewContext.HttpContext;
            S model = default(S);
            try
            {
                model = expression.Compile().Invoke(h.ViewData.Model);
            }
            catch { }
            
            var pres = transform.Transform(model);
            var prefix = TransformationsRegister.GetPrefix(transform.GetType());
            prefix = combinePrefixes(ExpressionHelper.GetExpressionText(expression), prefix);
            return Scope<I>(h, prefix, pres);
        }
        
        public static ScopeInfos<T> CurrentScope<T>(this IHtmlHelper h)
        {
            var scope = h.ViewData[RenderingScope.Field] as RenderingScope;
            if (scope != null) return scope.Infos<T>();
            else return null;
        }
        public static T Item<T>(this IHtmlHelper h)
        {
            var scope = h.ViewData[RenderingScope.Field] as RenderingScope;
            if (scope != null && scope.RawModel != null) return (T)scope.RawModel;
            else return default(T);
        }
        public static object Item(this IHtmlHelper h)
        {
            var scope = h.ViewData[RenderingScope.Field] as RenderingScope;
            if (scope != null && scope.RawModel != null) return scope.RawModel;
            else return null;
        }
        public static ScopeInfos<T, O> CurrentScope<T, O>(this IHtmlHelper h)
            where O : class
        {
            var scope = h.ViewData[RenderingScope.Field] as RenderingScope;
            if (scope != null)
            {
                return scope.TemplateInfos<T, O>();
                
            }
            else return null;
        }
    }
}
