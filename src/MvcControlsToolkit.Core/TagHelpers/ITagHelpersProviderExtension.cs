using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public interface ITagHelpersProviderExtension
    {
        Type For { get; }
        IEnumerable<KeyValuePair<string, 
            Func<TagHelperContext, 
                TagHelperOutput, 
                TagHelper, 
                TagProcessorOptions, 
                ContextualizedHelpers, Task>>>
            TagProcessors { get; }
        IEnumerable<KeyValuePair<string, DefaultTemplates>>
            Templates { get; }
    }
}
