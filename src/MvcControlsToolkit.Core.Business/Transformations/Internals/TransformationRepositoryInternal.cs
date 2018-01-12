using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Utilities;

namespace MvcControlsToolkit.Core.Business.Transformations.Internals
{
    public abstract class TransformationRepositoryInternal
    {

        public Func<object, object> GetGetKey(ICRUDRepository repo) => repo.GetKey;

        public abstract void Add<S>(ICRUDRepository repo, MappingContext context, bool full, params S[] viewModel);


        public void Delete<K>(ICRUDRepository repo, params K[] key)
        {
            repo.Delete<K>(key);
        }


        public abstract Task<D> GetById<D, K>(ICRUDRepository repo, MappingContext context, K key);


        public async Task<DataPage<D>> GetPage<D>(ICRUDRepository repo, Expression<Func<D, bool>> filter, Func<IQueryable<D>, IOrderedQueryable<D>> sorting, int page, int itemsPerPage, Func<IQueryable<D>, IQueryable<D>> grouping = null)
        {
            return await repo.GetPage<D>(filter, sorting, page, itemsPerPage, grouping);
        }


        public async Task<DataPage<T1>> GetPageExtended<T1, T2>(ICRUDRepository repo, Expression<Func<T1, bool>> filter, Func<IQueryable<T2>, IOrderedQueryable<T2>> sorting, int page, int itemsPerPage, Func<IQueryable<T1>, IQueryable<T2>> grouping = null) where T2 : T1
        {
            return
                await repo.GetPageExtended<T1, T2>(filter, sorting, page, itemsPerPage, grouping);
        }


        public async Task SaveChanges(ICRUDRepository repo)
        {
            await repo.SaveChanges();
        }

        public abstract void Update<D>(ICRUDRepository repo, MappingContext context, bool full, params D[] viewModel);


        public void UpdateKeys(ICRUDRepository repo)
        {
            repo.UpdateKeys();
        }
        public virtual async Task<DataPage<D>> ExecuteQuery<D, D1>(ICRUDRepository repo, MappingContext context, object query)
            where D1: D
        {
            return null;
        }
        public abstract void UpdateList<D>(ICRUDRepository repo, MappingContext context, bool full, IEnumerable<D> oldValues, IEnumerable<D> newValues);

    }
    public class TransformationRepositoryInternal<DTO> : TransformationRepositoryInternal
    {
        public override void Add<S>(ICRUDRepository repo, MappingContext context, bool full, params S[] viewModel)
        {
            if (viewModel == null) return;
            repo.Add<DTO>(full, viewModel.MapIEnumerable(context).To<DTO>().ToArray());
        }

        public override async Task<D> GetById<D, K>(ICRUDRepository repo, MappingContext context, K key)
        {
            var res = await repo.GetById<DTO, K>(key);
            if (res == null) return default(D);
            return res.Map<DTO>(context).To<D>();
        }

        public override void Update<D>(ICRUDRepository repo, MappingContext context, bool full, params D[] viewModel)
        {
            if (viewModel == null) return;
            repo.Update<DTO>(full, viewModel.MapIEnumerable(context).To<DTO>().ToArray());
        }
        public override void UpdateList<D>(ICRUDRepository repo, MappingContext context, bool full, IEnumerable<D> oldValues, IEnumerable<D> newValues)
        {
            repo.UpdateList<DTO>(full,
                oldValues?.MapIEnumerable(context).To<DTO>(),
                newValues?.MapIEnumerable(context).To<DTO>());
        }
    }
    
}
