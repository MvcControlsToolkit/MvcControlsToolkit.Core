using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpersUtilities
{
    public static class TagContextHelper
    {
        private const string bodyKey = "__body__";
        private const string rowContainerKey = "__row_container__";
        private const string bindingKeyPrefix = "__binding__";
        public static void OpenRowContainerContext(HttpContext httpContext)
        {
            RenderingContext.OpenContext<Tuple<IList<RowType>, IList<KeyValuePair<string, string>>>>(httpContext, rowContainerKey, null);
        }
        public static void CloseRowContainerContext(HttpContext httpContext, Tuple<IList<RowType>, IList<KeyValuePair<string, string>>> group)
        {
            RenderingContext.CloseContext<Tuple<IList<RowType>, IList<KeyValuePair<string, string>>>>(httpContext, rowContainerKey, group);
        }
        public static void RegisterRowsDependency(HttpContext httpContext, Action<IList<RowType>> action)
        {
            RenderingContext.AttachEvent<IList<RowType>>(httpContext, rowContainerKey, (r,o) => { action(r); });
        }
        public static void OpenBodyContext(HttpContext httpContext)
        {
            RenderingContext.OpenContext<Action<IHtmlContent, object>>(httpContext, bodyKey, (s, o) =>
            {
                (o as TagHelperOutput).PostContent.AppendHtml(s);
            });
        }
        public static void EndOfBodyHtml(HttpContext httpContext, IHtmlContent html)
        {
            var res = RenderingContext.Current(httpContext, bodyKey);
            if (res == null || res.Empty) OpenBodyContext(httpContext);
            RenderingContext.AttachEvent<Action<IHtmlContent, object>>(httpContext, bodyKey, 
                (f, o) =>
                {
                    f(html, o);
                }
                );
        }
        public static void CloseBodyContext(HttpContext httpContext, TagHelperOutput o)
        {
            RenderingContext.CloseContext(o, httpContext, bodyKey);
        }
        public static void RegisterDefaultToolWindow(HttpContext httpContext, Func<Tuple<IList<RowType>, IList<KeyValuePair<string, string>>>, IHtmlContent> getHtml)
        {
            RenderingContext.AttachEvent<Tuple<IList<RowType>, IList<KeyValuePair<string, string>>>>(httpContext, rowContainerKey,
                (groups, o) =>
                {
                    EndOfBodyHtml(httpContext, getHtml(groups));
                }
                );
        }
        public static void OpenBindingContext(HttpContext httpContext, string name, ModelExpression data)
        {
            RenderingContext.OpenContext<ModelExpression>(httpContext, bindingKeyPrefix+name, data);
        }
        public static void CloseBindingContext(HttpContext httpContext, string name)
        {
            RenderingContext.CloseContext(httpContext, bindingKeyPrefix + name);
        }
        public static ModelExpression GetBindingContext(HttpContext httpContext, string name)
        {
            return RenderingContext.CurrentData<ModelExpression>(httpContext, bindingKeyPrefix + name);
        }
    }
}
