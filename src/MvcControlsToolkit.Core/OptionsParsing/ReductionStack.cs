using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public static class ReductionStack
    {
        private const string reductionContextEntry = "_reduction_context_";
        public static ReductionContext GetFatherReductionContext(this TagHelperContext context, bool keep=false)
        {
            object res=null;
            context.Items.TryGetValue(reductionContextEntry, out res);
            if(! keep) context.Items[reductionContextEntry] = null;
            return res as ReductionContext;
        }
        public static void SetChildrenReductionContext(this TagHelperContext context, ReductionContext rc)
        {
            context.Items[reductionContextEntry] = rc; 
        }
    }
}
