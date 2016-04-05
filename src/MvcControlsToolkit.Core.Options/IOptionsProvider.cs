using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;



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
