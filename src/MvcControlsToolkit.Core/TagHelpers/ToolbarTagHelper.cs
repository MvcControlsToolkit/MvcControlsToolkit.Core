using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("toolbar", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ToolbarTagHelper: TagHelper
    {
        [HtmlAttributeName("zone-name")]
        public string ZoneName { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var rc = context.GetFatherReductionContext();
            string res = null;
            if (output.Content.IsModified) res = output.Content.GetContent();
            else res = (await output.GetChildContentAsync()).GetContent();
            rc.Results.Add(new ReductionResult(TagTokens.Content, 0, new KeyValuePair<string, IHtmlContent>(ZoneName,new HtmlString(res))));
        }
    }
}
