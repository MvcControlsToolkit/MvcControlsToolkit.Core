using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;
using MvcControlsToolkit.Core.Templates;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement(TagName)]
    public class CacheRowsTagHelper: RowTypeTagHelper
    {
        private const string TagName = "rows-collection";

        [HtmlAttributeName("rows-cache-key")]
        public string RowsCacheKey { get; set; }

        [HtmlAttributeName("tag-for-defaults")]
        public string TagHlperForDefaults { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        private IHttpContextAccessor contextAccessor;
        public CacheRowsTagHelper(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var rc = context.GetFatherReductionContext();
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(string.Empty);
            var currProvider = ViewContext.TagHelperProvider();
            var defaultTemplates = currProvider.GetDefaultTemplates(TagHlperForDefaults);
            //get row definitions
            IList<RowType> rows = string.IsNullOrEmpty(RowsCacheKey) ?
                null :
                RowType.GetRowsCollection(RowsCacheKey);
            var nc = new ReductionContext(TagTokens.RowContainer, 0, defaultTemplates, rows != null);
            context.SetChildrenReductionContext(nc);
            await output.GetChildContentAsync();
            var collector = new RowContainerCollector(nc);
            var res = collector.Process(this, defaultTemplates) as Tuple<IList<RowType>, IList<KeyValuePair<string, string>>>;
            if (rows == null)
            {
                rows = res.Item1;
                if (!string.IsNullOrEmpty(RowsCacheKey))
                    RowType.CacheRowGroup(RowsCacheKey, rows, contextAccessor.HttpContext);
            }
            var toolbars = res.Item2;
        }
    }
}
