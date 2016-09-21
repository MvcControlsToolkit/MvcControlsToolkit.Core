using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.TagHelpers;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public abstract class BaseCollector
    {
        public abstract object Process(Microsoft.AspNetCore.Razor.TagHelpers.TagHelper tag, DefaultTemplates defaults);
    }
}
