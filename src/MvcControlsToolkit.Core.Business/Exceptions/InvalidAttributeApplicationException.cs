using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Exceptions
{
    public class InvalidAttributeApplicationException: Exception
    {
        public InvalidAttributeApplicationException(string message) : base(message)
        {
        }
    }
}
