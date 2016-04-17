using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvcControlsToolkit.Core.ModelBinding;

namespace MvcControlsToolkit.Core.Extensions
{
    public class CoreMiddleware
    {
        private readonly RequestDelegate _next;


        public CoreMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var tr=context.RequestServices.GetService<RequestTransformationsRegister>();
            tr.Fill(context.Request);
            await _next.Invoke(context);
        }
    }
}
