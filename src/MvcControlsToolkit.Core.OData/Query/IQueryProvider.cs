using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData
{
    public interface IWebQueryProvider
    {
        QueryDescription<T> Parse<T>();
    }
}
