using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Options.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OptionNameAttribute: Attribute
    {
        public string Name { get; set; }

        public OptionNameAttribute (string name)
        {
            Name = name;
        }
    }
}
