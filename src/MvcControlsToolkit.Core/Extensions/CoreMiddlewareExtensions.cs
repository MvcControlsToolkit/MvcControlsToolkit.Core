using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.OptionsModel;
using MvcControlsToolkit.Core.Options.Extensions;

namespace MvcControlsToolkit.Core.Extensions
{
    public static class CoreMiddlewareExtensions
    {
        private static bool initialized = false;
        public static IApplicationBuilder UseMvcControlsToolkit(this IApplicationBuilder builder)
        {
            if (initialized) return builder;
            initialized = true;
            var options = builder.ApplicationServices.GetService(typeof(IOptions<MvcControlsToolkitOptions>)) as IOptions<MvcControlsToolkitOptions>;
            MvcControlsToolkitOptions.Instance = options.Value;
            builder.UsePreferences();
            return builder.UseMiddleware<CoreMiddleware>();
        }
    }
}
