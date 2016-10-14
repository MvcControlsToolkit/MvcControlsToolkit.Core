using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business
{
    public interface IUpdateConnections
    {
        bool MayUpdate(string prefix);
    }
}
