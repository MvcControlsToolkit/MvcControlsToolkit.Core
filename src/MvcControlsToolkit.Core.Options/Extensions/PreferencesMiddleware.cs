using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MvcControlsToolkit.Core.Options.Extensions
{
    public class PreferencesMiddleware
    {
        
            private readonly RequestDelegate _next;
            

            public PreferencesMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
            {
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                var preferencesHandler=context.RequestServices.GetService(typeof(IPreferencesService)) as IPreferencesService;
                preferencesHandler.BindToRequest(context);
                await _next.Invoke(context);
            }
        
    }
}
