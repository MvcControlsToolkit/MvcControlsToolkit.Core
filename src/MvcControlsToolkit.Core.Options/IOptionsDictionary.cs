using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Options
{
    public interface IOptionsDictionary
    {
        IOptionsProvider AddOption(IOptionsProvider provider, string name, string value, uint? priority=null);
        void Remove(string name);

        Func<Type, object> Creator{get; set;}

        object GetOptionObject(string prefix, Type type, object instance = null);

        List<KeyValuePair<string, string>> GetEntries(string prefix, string newPrefix=null);

        List<IOptionsProvider> AddOptionObject(IOptionsProvider provide, string prefix, object x, uint? priority = null, uint jumpComplexTypes = uint.MaxValue);
    }
}
