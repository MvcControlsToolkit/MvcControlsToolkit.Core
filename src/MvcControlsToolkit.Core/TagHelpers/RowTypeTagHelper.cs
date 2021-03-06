﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using MvcControlsToolkit.Core.OptionsParsing;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("row-type", Attributes = ForAttributeName)]
    public class RowTypeTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string KeyAttributeName = "asp-for";
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }
        [HtmlAttributeName("row-id")]
        public string RowId { get; set; }
        [HtmlAttributeName("key")]
        public ModelExpression KeyName { get; set; }
        [HtmlAttributeName("mvc-controller")]
        public Type ControllerType { get; set; }
        [HtmlAttributeName("title")]
        public string RowTitle { get; set; }
        [HtmlAttributeName("all-properties")]
        public bool AllProperties { get; set; }
        [HtmlAttributeName("from-row")]
        public uint? FromRow { get; set; }
        [HtmlAttributeName("custom-buttons")]
        public bool CustomButtons { get; set; }
        [HtmlAttributeName("operations")]
        public Func<IPrincipal, Functionalities> RequiredFunctionalities { get; set; }
        [HtmlAttributeName("client-operations")]
        public string ClientRequiredFunctionalities { get; set; }
        [HtmlAttributeName("row-class")]
        public string RowCssClass { get; set; }
        [HtmlAttributeName("input-class")]
        public string InputCssClass { get; set; }
        [HtmlAttributeName("chekbox-class")]
        public string CheckboxCssClass { get; set; }
        [HtmlAttributeName("localization-type")]
        public Type LocalizationType { get; set; }
        [HtmlAttributeName("enable-query")]
        public bool? QueryEnabled { get; set; }
        [HtmlAttributeName("sorting-clauses")]
        public uint MaxSortingClauses { get; set; }
        public IStringLocalizerFactory DefaultLocalizerFactory {get; set;}
        public RowTypeTagHelper(IStringLocalizerFactory factory)
        {
            DefaultLocalizerFactory = factory;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (For == null) throw new ArgumentNullException(ForAttributeName);
            if (KeyName == null && !FromRow.HasValue) throw new ArgumentNullException(KeyAttributeName);
            var rc = context.GetFatherReductionContext();
            if(rc.RowParsingDisabled)
            {
                output.TagName = string.Empty;
                output.Content.SetHtmlContent(string.Empty);
                return;
            }
            var nc = new ReductionContext(TagTokens.Row, 0, rc.Defaults);
            context.SetChildrenReductionContext(nc);
            await output.GetChildContentAsync();
            RowType inherit = null;
            if (FromRow.HasValue)
            {
                var count = 0;
                foreach (var item in rc.Results)
                {
                    if(item.Token == TagTokens.Row)
                    {
                        if(FromRow.Value==count)
                        {
                            inherit = item.Result as RowType;
                            continue;
                        }
                        else count++;
                    }
                }
            }
            var collector = new RowCollector(nc, FromRow);
            var res = collector.Process(this, rc.Defaults);
            if (res != null) rc.Results.Add(new ReductionResult(TagTokens.Row, 0, res));
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(string.Empty);
        }
    }
}
