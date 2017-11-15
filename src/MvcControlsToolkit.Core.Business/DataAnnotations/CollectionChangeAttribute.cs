using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum CollectionChangeMode {Add, Replace}

namespace MvcControlsToolkit.Core.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CollectionChangeAttribute: Attribute
    {
        public CollectionChangeMode Mode { get; set; }
        public CollectionChangeAttribute(CollectionChangeMode mode = CollectionChangeMode.Replace)
        {
            Mode = mode;
            
        }

    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CollectionKeyAttribute : Attribute
    {
        public string KeyName { get; private set; }
        public CollectionKeyAttribute(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentException(nameof(keyName));
            KeyName = keyName;
        }

    }
}
