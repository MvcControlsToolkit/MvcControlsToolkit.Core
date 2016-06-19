using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace MvcControlsToolkit.Core.Options.Extensions
{
    public static class PreferencesMiddlewareExtensions
    {
        private static bool initialized = false;
        public static IApplicationBuilder UsePreferences(this IApplicationBuilder builder)
        {
            if (initialized) return builder;
            initialized = true;
            return builder.UseMiddleware<PreferencesMiddleware>();
        }
    }
}
