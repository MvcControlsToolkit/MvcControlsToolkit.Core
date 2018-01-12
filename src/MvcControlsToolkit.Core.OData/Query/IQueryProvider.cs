using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData
{
    public interface IWebQueryProvider
    {
        QueryDescription<T> Parse<T>();
        string Filter { get; set; }
        
        string OrderBy { get; set; }
        
        string Apply { get; set; }
        
        string Search { get; set; }
        
        string Skip { get; set; }
        
        string Top { get; set; }
    }
    public interface IWebQueryable
    {
        Task<DataPage<D>> ExecuteQuery<D, Dext>(IWebQueryProvider query)
            where Dext: D;

    }
}
