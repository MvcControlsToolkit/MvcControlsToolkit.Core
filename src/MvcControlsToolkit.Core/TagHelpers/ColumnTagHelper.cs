using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.OptionsParsing;


namespace MvcControlsToolkit.Core.TagHelpers
{
    
    public abstract class ColumnBaseTagHelper : TagHelper
    {
        

        

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
        [HtmlAttributeName("detail-widths")]
        public decimal[] DetailWidths { get; set; }
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
        [HtmlAttributeName("input-class")]
        public string InputDetailCssClass { get; set; }
        [HtmlAttributeName("chekbox-class")]
        public string CheckboxDetailCssClass { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            
            var rc = context.GetFatherReductionContext();
            if (rc.RowParsingDisabled)
            {
                output.TagName = string.Empty;
                output.Content.SetHtmlContent(string.Empty);
                return;
            }
            var nc = new ReductionContext(TagTokens.Column, 0, rc.Defaults);
            context.SetChildrenReductionContext(nc);
            await output.GetChildContentAsync();
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(string.Empty);
            var collector = new ColumnCollector(nc);
            rc.Results.Add(new ReductionResult(TagTokens.Column, Remove ? -1 : 1, collector.Process(this, rc.Defaults)));
        }

    }
    [HtmlTargetElement("column", Attributes = ForAttributeName)]
    public class ColumnTagHelper : ColumnBaseTagHelper
    {
        private const string ForAttributeName = "asp-for";
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }
        [HtmlAttributeName("query-constraints")]
        public QueryOptions? Queries { get; set; }
        [HtmlAttributeName("filter-clauses")]
        public QueryOptions[] FilterClauses { get; set; }
    }

    [HtmlTargetElement("column", Attributes = NameAttributeName)]
    public class CustomColumnTagHelper : ColumnBaseTagHelper
    {
        private const string NameAttributeName = "name";
        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }
    }
}
