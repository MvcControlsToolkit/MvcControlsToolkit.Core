using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.Options.Extensions;
using MvcControlsToolkit.Core.OData;
using MvcControlsToolkit.Core.Options.Providers;

namespace MvcControlsToolkit.Core.Extensions
{
    public static class ODataDIExtensions
    {
        public static IServiceCollection AddODataQueries(this IServiceCollection services)
        {
            services.AddPreferences()
                .AddPreferencesClass<IWebQueryProvider, ODataQueryProvider>("Request.Query.OData")
                .AddPreferencesProvider(new ODataQueryOptionsProvider("Request.Query.OData"));
            return services;
                
        }
    }
}
