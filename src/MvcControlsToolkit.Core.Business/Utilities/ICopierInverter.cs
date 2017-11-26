using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public interface ICopierInverter
    {
        LambdaExpression InvertPropertyChain(List<PropertyInfo> chain);
    }
}
