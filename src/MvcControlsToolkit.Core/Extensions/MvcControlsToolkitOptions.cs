using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.TagHelpers;

namespace MvcControlsToolkit.Core.Extensions
{
    public class MvcControlsToolkitOptions
    {
        internal static MvcControlsToolkitOptions Instance = null;
        public Type CustomMessagesResourceType { get; set;}

        public ITagHelpersProvider DefaultProvider { get; set; }

    }
}
