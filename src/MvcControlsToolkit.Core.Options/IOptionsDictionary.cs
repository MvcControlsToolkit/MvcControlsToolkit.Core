using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Options
{
    public interface IOptionsDictionary
    {
        void AddOption(string name, string value, uint? priority=null);
        void Remove(string name);

        object GetOptionObject(string prefix, Type type);

        void AddOptionObject(string prefix, object x, uint? priority = null);
    }
}
