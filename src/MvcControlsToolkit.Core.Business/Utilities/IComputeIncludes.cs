using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public interface IComputeIncludes<D>
    {
        Func<IQueryable<D>, IQueryable<D>>  GetIncludes();
    }
}
