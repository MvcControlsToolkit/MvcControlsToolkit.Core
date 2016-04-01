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
        string Prefix { get; set; }
        bool CanSave { get; set; }
        void Save(HttpContext ctx, IOptionsDictionary dict);

        void Load(HttpContext ctx, IOptionsDictionary dict);
        
    }
}
