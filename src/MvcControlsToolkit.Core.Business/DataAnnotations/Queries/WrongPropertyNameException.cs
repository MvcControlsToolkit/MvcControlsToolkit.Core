using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    public class WrongPropertyNameException: Exception
    {
        private string _PropertyName;
        public string PropertyName { get { return _PropertyName; } }
        public WrongPropertyNameException(string propertyName)
        {
            _PropertyName = propertyName;
        }
        public override string Message
        {
            get
            {
                return string.Format(Resources.NoPublicProperty, PropertyName);
            }
        }
    }
}
