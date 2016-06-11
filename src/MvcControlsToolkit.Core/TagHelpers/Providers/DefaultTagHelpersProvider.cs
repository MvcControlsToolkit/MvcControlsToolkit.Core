using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MvcControlsToolkit.Core.TagHelpers.Providers
{
    public class DefaultTagHelpersProvider : ITagHelpersProvider
    {
        public bool GenerateNames
        {
            get
            {
                return true;
            }

            
        }

        public bool RequireUnobtrusiveValidation
        {
            get
            {
                return true;
            }


        }

        public void PrepareViewContext(ViewContext context)
        {
            
        }

        public void UnPrepareViewContext(ViewContext context)
        {
            
        }
    }
}
