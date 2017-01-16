using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData
{
    interface IQueryProvider
    {
        QueryDescription<T> Parse<T>();
    }
}
