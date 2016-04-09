using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Mvc;

namespace MvcControlsToolkit.Core.Extensions
{
    public static class CoreDIExtensions
    {
        public static IServiceCollection AddMvcControlsToolkit(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<MvcOptions>, MvcControlsToolkitOptionsSetup>();
            return services;
        }
    }
}
