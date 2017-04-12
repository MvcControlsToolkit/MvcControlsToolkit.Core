using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("only-once", Attributes = ForAttributeName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class OnlyOnceTagHelper : TagHelper
    {
        private const string hashKey = "_hash_key_";
        private const string ForAttributeName = "asp-for";
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }


        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private IHttpContextAccessor httpContextAccessor;

        public OnlyOnceTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {

            if (For == null && For.Name != null) throw new ArgumentException("ForAttributeName");
            output.TagName = string.Empty;
            string fullName = ViewContext.ViewData.GetFullHtmlFieldName(For.Name);
            var httpContext = httpContextAccessor.HttpContext;
            bool found = false;
            object hashSet;
            
            if (httpContext.Items.TryGetValue(hashKey, out hashSet))
            {
                HashSet<string> res = hashSet as HashSet<string>;
                if (res != null && res.Contains(fullName)) found = true;
                else res.Add(fullName);
            }
            else
            {
                HashSet<string> res = new HashSet<string>();
                res.Add(fullName);
                httpContext.Items[hashKey] = res;
            }
            if(found)
            {
                output.Content.SetContent(null);
            }
        }
    }
}
