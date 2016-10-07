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
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(string.Empty);
            var rc = context.GetFatherReductionContext();
            string res = null;
            res = (await output.GetChildContentAsync()).GetContent();
            rc.Results.Add(new ReductionResult(TagTokens.Content, 0, new KeyValuePair<string, string>(ZoneName,res)));
            
        }
    }
}
