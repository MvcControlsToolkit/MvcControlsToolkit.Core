using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;

namespace MvcControlsToolkit.Core.Options.Extensions
{
    public static class PreferencesMiddlewareExtensions
    {
        public static IApplicationBuilder UsePreferences(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PreferencesMiddleware>();
        }
    }
}
