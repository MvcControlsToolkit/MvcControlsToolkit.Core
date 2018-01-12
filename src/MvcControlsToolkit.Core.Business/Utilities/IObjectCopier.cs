using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public interface IObjectCopier
    {
        object Copy(object source, object destination);
    }
    
    public interface IObjectCopier<D>
    {
        D Copy(object source, D destination);
    }

    public interface IObjectCopier<S, D>
    {
        D Copy(S source, D destination);
    }
}
