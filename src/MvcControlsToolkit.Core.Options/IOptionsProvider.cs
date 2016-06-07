using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;



namespace MvcControlsToolkit.Core.Options
{
    public interface IOptionsProvider
    {
        string Prefix { get; set;}
        uint Priority { get; set; }
        bool CanSave { get;}
        bool Enabled(HttpContext ctx);
        bool AutoSave { get; set; }
        bool AutoCreate { get; set; }
        void Save(HttpContext ctx, IOptionsDictionary dict);

        List<IOptionsProvider> Load(HttpContext ctx, IOptionsDictionary dict);
        
    }
}
