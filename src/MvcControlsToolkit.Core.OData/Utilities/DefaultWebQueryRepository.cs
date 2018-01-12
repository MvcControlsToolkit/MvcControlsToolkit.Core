using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.OData;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class DefaultWebQueryRepository : ICRUDRepository, IWebQueryable
    {
        protected ICRUDRepository SourceRepository { get; private set; }
        public DefaultWebQueryRepository(ICRUDRepository sourceRepository)
        {
            SourceRepository = sourceRepository;
        }
        public Func<object, object> GetKey => SourceRepository.GetKey;

        public void Add<T>(bool full, params T[] viewModel)
        {
            SourceRepository.Add(full, viewModel);
        }

        public void Delete<U>(params U[] key)
        {
            SourceRepository.Delete(key);
        }

        public async Task<DataPage<D>> ExecuteQuery<D, Dext>(IWebQueryProvider query) where Dext : D
        {
            if (SourceRepository is IWebQueryable)
                return await (SourceRepository as IWebQueryable).ExecuteQuery<D, Dext>(query);
            else
            {
                QueryDescription<D> qd = null;
                if (query != null)
                {
                    qd = query.Parse<D>();
                }

                if (qd == null) return await
                        SourceRepository.GetPage<D>(null, null, 1, int.MaxValue, null);
                else
                {
                    var grouping = qd.GetGrouping<Dext>();
                    if (grouping == null)
                    {
                        return await
                            SourceRepository.GetPage<D>(qd.GetFilterExpression(), qd.GetSorting(), (int)qd.Page, (int)qd.Take);
                    }
                    else
                    {
                        return await
                            SourceRepository.GetPageExtended<D, Dext>(qd.GetFilterExpression(), qd.GetSorting<Dext>(), (int)qd.Page, (int)qd.Take, grouping);
                    }
                }
            }
        }

        public async Task<T> GetById<T, U>(U key)
        {
            return await SourceRepository.GetById<T, U>(key);
        }

        public async Task<DataPage<T>> GetPage<T>(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> sorting, int page, int itemsPerPage, Func<IQueryable<T>, IQueryable<T>> grouping = null)
        {
            return await SourceRepository.GetPage<T>(filter, sorting, page, itemsPerPage,  grouping);
        }

        public async Task<DataPage<T1>> GetPageExtended<T1, T2>(Expression<Func<T1, bool>> filter, Func<IQueryable<T2>, IOrderedQueryable<T2>> sorting, int page, int itemsPerPage, Func<IQueryable<T1>, IQueryable<T2>> grouping = null) where T2 : T1
        {
            return await SourceRepository.GetPageExtended<T1, T2>(filter, sorting, page, itemsPerPage, grouping);
        }

        public async Task SaveChanges()
        {
            await SourceRepository.SaveChanges();
        }

        public void Update<T>(bool full, params T[] viewModel)
        {
            SourceRepository.Update(full, viewModel);
        }

        public void UpdateKeys()
        {
            SourceRepository.UpdateKeys();
        }

        public void UpdateList<T>(bool full, IEnumerable<T> oldValues, IEnumerable<T> newValues)
        {
            SourceRepository.UpdateList(full, oldValues, newValues);
        }
    }
}
