using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace MvcControlsToolkit.Core.Templates
{
    public class LayoutTemplateOptions
    {
        public IList<RowType> Rows { get; private set;}
        public IList<KeyValuePair<string, IHtmlContent>> Toolbars  { get; private set; }
        public IEnumerable<Template<LayoutTemplateOptions>> SubTemplates { get; private set; }
        public Template<LayoutTemplateOptions> LayoutTemplate { get; private set; }
        public IHtmlContent MainContent { get; private set; }

        public LayoutTemplateOptions (
            IList<RowType> rows,
            IList<KeyValuePair<string, IHtmlContent>> toolbars,
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
