using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.TagHelpersUtilities;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [HtmlTargetElement("body")]
    public class RenderAtEndOfBodyTagHelper: TagHelper
    {
        public override int Order => -10000;
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
            TagContextHelper.OpenBodyContext(contextAccessor.HttpContext, output);
            await output.GetChildContentAsync();
            TagContextHelper.CloseBodyContext(contextAccessor.HttpContext);
        }
    }
}
