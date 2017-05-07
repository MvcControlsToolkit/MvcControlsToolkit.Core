using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Extensions
{
    public interface IDIMeta
    {
        bool IsRegistred(Type t);
        Type RegistredTypeFor(Type t);
    }
}
