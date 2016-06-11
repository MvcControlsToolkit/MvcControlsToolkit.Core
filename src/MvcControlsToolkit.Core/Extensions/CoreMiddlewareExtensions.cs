﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using MvcControlsToolkit.Core.Options.Extensions;
using MvcControlsToolkit.Core.Views;

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
            BasicTransformationsRegistration.Registration();
            return builder.UseMiddleware<CoreMiddleware>();
        }
    }
}
