using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public interface ITagHelpersExtension
    {
        IEnumerable<ITagHelpersProviderExtension> ProviderExtensions { get; }
        IEnumerable<ITagHelpersProvider> Providers { get; }
        bool HasEmbeddedFiles { get; }
    }
}
