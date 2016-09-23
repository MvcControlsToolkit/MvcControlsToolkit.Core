using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public abstract class ExternalKeyTagHelper: TagHelper
    {
        [HtmlAttributeName("display-property")]
        public ModelExpression DisplayProperty { get; set; }
        [HtmlAttributeName("items-display-property")]
        public string ItemsDisplayProperty { get; set; }
        [HtmlAttributeName("items-value-property")]
        public string ItemsValueProperty { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (DisplayProperty == null) new ArgumentNullException("display-property");
            if (string.IsNullOrWhiteSpace(ItemsDisplayProperty)) new ArgumentNullException("items-display-property");
            if (string.IsNullOrWhiteSpace(ItemsValueProperty)) new ArgumentNullException("items-value-property");
            var rc = context.GetFatherReductionContext();
            
            rc.Results.Add(new ReductionResult(TagTokens.ExternalKeyConnection, 0, GetExpernalConnection()));
        }
        protected abstract ColumnConnectionInfos GetExpernalConnection();
        
    }
    [HtmlTargetElement("external-key-remote", ParentTag = "column", TagStructure = TagStructure.WithoutEndTag)]
    public class ExternalKeyRemoteTagHelper : ExternalKeyTagHelper
    {
        
        [HtmlAttributeName("items-url")]
        public string ItemsUrl { get;  set; }
        [HtmlAttributeName("url-token")]
        public string UrlToken { get;  set; }
        [HtmlAttributeName("dataset-name")]
        public string DataSetName { get; set; }
        [HtmlAttributeName("max-results")]
        public uint MaxResults { get; set; }
        protected override ColumnConnectionInfos GetExpernalConnection()
        {
            if (string.IsNullOrWhiteSpace(ItemsUrl)) new ArgumentNullException("items-url");
            if (string.IsNullOrWhiteSpace(UrlToken)) new ArgumentNullException("url-token");
            if (string.IsNullOrWhiteSpace(DataSetName)) new ArgumentNullException("max-results");
            return new ColumnConnectionInfosOnLine
                (DisplayProperty, 
                ItemsDisplayProperty, 
                ItemsValueProperty, 
                ItemsUrl, 
                UrlToken,
                MaxResults,
                DataSetName);
        }

    }
    [HtmlTargetElement("external-key-static", ParentTag = "column", TagStructure = TagStructure.WithoutEndTag)]
    public class ExternalKeyStaticTagHelper : ExternalKeyTagHelper
    {

        [HtmlAttributeName("client-items-selector")]
        public string ClientItemsSelector { get;  set; }
        [HtmlAttributeName("items-selector")]
        public Func<object,Task<IEnumerable>> ItemsSelector { get; set; }

    protected override ColumnConnectionInfos GetExpernalConnection()
        {
            if (string.IsNullOrWhiteSpace(ClientItemsSelector) && ItemsSelector==null) new ArgumentNullException("client-items/client-items-selector");
            
            return new ColumnConnectionInfosStatic
                (DisplayProperty,
                ItemsDisplayProperty,
                ItemsValueProperty,
                ItemsSelector,
                ClientItemsSelector);
        }

    }
}
