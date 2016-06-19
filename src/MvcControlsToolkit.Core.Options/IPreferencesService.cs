using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MvcControlsToolkit.Core.Options
{
    public interface IPreferencesService
    {
        T BuildOptionsObject<T>()
            where T : class, new();
        void BindToRequest(HttpContext ctx);

        void Save<T>(T options);
    }
}
