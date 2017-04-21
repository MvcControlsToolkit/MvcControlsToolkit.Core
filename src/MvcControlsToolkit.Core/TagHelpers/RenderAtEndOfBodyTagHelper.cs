using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MvcControlsToolkit.Core.TagHelpers
{
    
    [HtmlTargetElement("body", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RenderAtEndOfBodyTagHelper: TagHelper
    {
        public override int Order => int.MinValue;
        private IHttpContextAccessor contextAccessor;
        public RenderAtEndOfBodyTagHelper(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            
            await output.GetChildContentAsync();
            TagContextHelper.CloseBodyContext(contextAccessor.HttpContext, output);
        }
    }
}
