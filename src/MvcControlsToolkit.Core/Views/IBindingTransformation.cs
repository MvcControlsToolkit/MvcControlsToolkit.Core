using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Views
{
    public interface IBindingTransformation
    {
        Microsoft.AspNetCore.Http.HttpContext Context { set; }
    }
    public interface IBindingTransformation<S, I, D>: IBindingTransformation
    {
        I Transform(S x);
        D InverseTransform(I x);
    }
}
