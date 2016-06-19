using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public interface ITagHelpersProvider
    {
        bool GenerateNames { get; }
        bool RequireUnobtrusiveValidation { get; }

        void PrepareViewContext(ViewContext context);

        void UnPrepareViewContext(ViewContext context);
        //to be extended
    }
}
