using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;


namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("column", Attributes = ForAttributeName)]
    public class ColumnTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("remove")]
        public bool Remove { get; set; }

        [HtmlAttributeName("title")]
        public string ColumnTitle { get; set; }
        [HtmlAttributeName("hidden")]
        public bool? Hidden { get; set; }
        [HtmlAttributeName("readonly")]
        public bool ReadOnly { get; set; }
        [HtmlAttributeName("editonly")]
        public bool EditOnly { get; set; }
        [HtmlAttributeName("format")]
        public string DisplayFormat { get; set; }
        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }
        [HtmlAttributeName("null-text")]
        public string NullDisplayText { get; set; }
        [HtmlAttributeName("description")]
        public string Description { get; set; }
        [HtmlAttributeName("widths")]
        public decimal[] Widths { get; set; }
        [HtmlAttributeName("priority")]
        public int? ColumnOrder { get; set; }
        [HtmlAttributeName("colspan")]
        public int? ColSpan { get; set; }
        [HtmlAttributeName("container-class")]
        public string ColumnCssClass { get; set; }
        [HtmlAttributeName("input-class")]
        public string InputCssClass { get; set; }
        [HtmlAttributeName("chekbox-class")]
        public string CheckboxCssClass { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (For == null) throw new ArgumentNullException(ForAttributeName);
            var rc = context.GetFatherReductionContext();
            var nc = new ReductionContext(TagTokens.Column, 0, rc.Defaults);
            context.SetChildrenReductionContext(nc);
            await output.GetChildContentAsync();

            var collector = new ColumnCollector(nc);
            rc.Results.Add(new ReductionResult(TagTokens.Column, Remove ? -1 : 1, collector.Process(this, rc.Defaults)));
        }

    }
}
