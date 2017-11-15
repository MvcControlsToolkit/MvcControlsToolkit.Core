using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    interface IComputeConnections
    {
        IEnumerable<PropertyInfo> GetNeededConnections(IUpdateConnections x);
    }
}
