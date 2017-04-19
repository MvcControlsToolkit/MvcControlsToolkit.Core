using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace MvcControlsToolkit.Core.Templates
{
    public class LayoutTemplateOptions
    {
        public IList<RowType> Rows { get; protected set;}
        public IList<KeyValuePair<string, string>> Toolbars  { get; protected set; }
        public IEnumerable<Template<LayoutTemplateOptions>> SubTemplates { get; protected set; }
        public Template<LayoutTemplateOptions> LayoutTemplate { get; protected set; }
        public IHtmlContent MainContent { get; protected set; }

        public LayoutTemplateOptions (
            IList<RowType> rows,
            IList<KeyValuePair<string, string>> toolbars,
            Template<LayoutTemplateOptions> layoutTemplate,
            IEnumerable<Template<LayoutTemplateOptions>> subTemplates,
            IHtmlContent mainContent)
        {
            Rows = rows;
            Toolbars = toolbars;
            LayoutTemplate = layoutTemplate;
            SubTemplates = subTemplates;
            MainContent = mainContent;
        }
    }
}
