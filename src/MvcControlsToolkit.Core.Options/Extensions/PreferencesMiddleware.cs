using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace MvcControlsToolkit.Core.Options.Extensions
{
    public class PreferencesMiddleware
    {
        public class RequestPreferencesMiddleware
        {
            private readonly RequestDelegate _next;
            

            public RequestPreferencesMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
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
}
