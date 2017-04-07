using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpersUtilities
{
    public static class TagContextHelper
    {
        private const string bodyKey = "__body__";
        private const string rowContainerKey = "__row_container__";

        public static void OpenRowContainerContext(HttpContext httpContext)
        {
            RenderingContext.OpenContext<IList<RowType>>(httpContext, rowContainerKey, null);
        }
        public static void CloseRowContainerContext(HttpContext httpContext, IList<RowType> rows)
        {
            RenderingContext.CloseContext<IList<RowType>>(httpContext, rowContainerKey, rows);
        }
        public static void RegisterRowsDependency(HttpContext httpContext, Action<IList<RowType>> action)
        {
            RenderingContext.AttachEvent(httpContext, rowContainerKey, action);
        }
        public static void OpenBodyContext(HttpContext httpContext, TagHelperOutput output)
        {
            RenderingContext.OpenContext<Action<string>>(httpContext, bodyKey, s =>
            {
                output.PostContent.AppendHtml(s);
            });
        }
        public static void EndOfBodyHtml(HttpContext httpContext, string html)
        {
            RenderingContext.AttachEvent<Action<string>>(httpContext, bodyKey, 
                f =>
                {
                    f(html);
                }
                );
        }
        public static void CloseBodyContext(HttpContext httpContext)
        {
            RenderingContext.CloseContext(httpContext, bodyKey);
        }
        public static void RegisterDefaultToolWindow(HttpContext httpContext, Func<IList<RowType>, string> getHtml)
        {
            RenderingContext.AttachEvent<IList<RowType>>(httpContext, rowContainerKey,
                rows =>
                {
                    EndOfBodyHtml(httpContext, getHtml(rows));
                }
                );
        }
    }
}
