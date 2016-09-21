using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.Views
{
    public static class TemplateExtensions
    {
        public static ScopeInfos<T, Column> ColumnScope<T>(this IHtmlHelper helper)
        {
            return helper.CurrentScope<T, Column>();
        }
        public static ScopeInfos<T, RowType> RowScope<T>(this IHtmlHelper helper)
        {
            return helper.CurrentScope<T, RowType>();
        }
    }
}
