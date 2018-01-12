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
        [HtmlAttributeName("query-display")]
        public bool QueryDisplay { get;  set; }
        [HtmlAttributeName("items-provider-type")]
        public Type ProviderType { get; set; }
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
            if (string.IsNullOrWhiteSpace(ItemsUrl) && ProviderType == null) new ArgumentNullException("items-url/items-provider-type");
            if (string.IsNullOrWhiteSpace(UrlToken)) UrlToken="_ s";
            if (string.IsNullOrWhiteSpace(DataSetName)) DataSetName = DisplayProperty.Name + "_DataSet";
            if (MaxResults==0) MaxResults=10;
            return new ColumnConnectionInfosOnLine
                (DisplayProperty, 
                ItemsDisplayProperty, 
                ItemsValueProperty, 
                ItemsUrl, 
                UrlToken,
                MaxResults,
                DataSetName,
                QueryDisplay);
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
            if (string.IsNullOrWhiteSpace(ClientItemsSelector) && ItemsSelector==null&& ProviderType == null) new ArgumentNullException("client-items/client-items-selector/items-provider-type");
            
            return new ColumnConnectionInfosStatic
                (DisplayProperty,
                ItemsDisplayProperty,
                ItemsValueProperty,
                ItemsSelector,
                ClientItemsSelector,
                QueryDisplay);
        }

    }
}
