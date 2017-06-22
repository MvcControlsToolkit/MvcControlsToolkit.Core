using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("form", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RenderAtEndOfFormTagHelper: TagHelper
    {
        public override int Order => int.MaxValue;
        private IHttpContextAccessor contextAccessor;
        public RenderAtEndOfFormTagHelper(IHttpContextAccessor contextAccessor)
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
            if(!output.Content.IsModified)
                await output.GetChildContentAsync();

            TagContextHelper.CloseFormContext(contextAccessor.HttpContext, output);
        }
    }
}
