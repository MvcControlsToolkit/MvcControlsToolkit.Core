using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MvcControlsToolkit.Core.TagHelpers.Providers;

namespace MvcControlsToolkit.Core.Extensions
{
    public class MvcControlsToolkitSetup : IConfigureOptions<MvcControlsToolkitOptions>
    {
        public void Configure(MvcControlsToolkitOptions options)
        {
            options.DefaultProvider = new DefaultTagHelpersProvider();
        }
    }
}
