using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public interface ICRUDRepository
    {
        void Update<T>(bool full, params T[] viewModel);
        void UpdateList<T>(bool full, IEnumerable<T> oldValues, IEnumerable<T> newValues);
        void Add<T>(bool full, params T[] viewModel);
        void Delete<U>(params U[] key);

        Task<T> GetById<T, U>(U key);

        Task<DataPage<T>> GetPage<T>(Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> sorting,
            int page, int itemsPerPage, Func<IQueryable<T>, IQueryable<T>> grouping=null);

        Task SaveChanges();
        void UpdateKeys();

        Func<object, object> GetKey { get; }
        
    }
}
