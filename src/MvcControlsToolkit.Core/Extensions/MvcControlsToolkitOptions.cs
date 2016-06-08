using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Extensions
{
    public class MvcControlsToolkitOptions
    {
        internal static MvcControlsToolkitOptions Instance = null;
        public Type CustomMessagesResourceType { get; set;}
    }
}
