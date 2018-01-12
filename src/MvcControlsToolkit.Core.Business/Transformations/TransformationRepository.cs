using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Transformations.Internals;
using MvcControlsToolkit.Core.Business.Utilities;

namespace MvcControlsToolkit.Core.Business.Transformations
{
    public class TransformationRepository : ICRUDRepository
    {
        private IDictionary<Type, TransformationRepositoryInternal> allowedMappings;
        protected ICRUDRepository repository;
        protected MappingContext context;
        public TransformationRepository(ICRUDRepository repository, MappingContext context, IDictionary<Type, TransformationRepositoryInternal> allowedMappings)
        {
            this.repository = repository;
            this.allowedMappings = allowedMappings;
            this.context = context;
        }

        protected TransformationRepositoryInternal findMap<T>()
        {
            TransformationRepositoryInternal res = null;
            if (!allowedMappings.TryGetValue(typeof(T), out res)) return null;
            return res;
        }

        public Func<object, object> GetKey => repository.GetKey;


        public void Add<T>(bool full, params T[] viewModel)
        {
            var prep = findMap<T>();
            if (prep == null) repository.Add<T>(full, viewModel);
            else prep.Add(repository, context, full, viewModel);
        }

        public void Delete<U>(params U[] key)
        {
            repository.Delete(key);
        }

        public async Task<T> GetById<T, U>(U key)
        {
            var prep = findMap<T>();
            if (prep == null) return await repository.GetById<T, U>(key);
            else return await prep.GetById<T, U>(repository, context, key);
        }

        public async Task<DataPage<T>> GetPage<T>(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> sorting, int page, int itemsPerPage, Func<IQueryable<T>, IQueryable<T>> grouping = null)
        {
            return await repository.GetPage<T>(filter, sorting, page, itemsPerPage, grouping);
        }

        public async Task<DataPage<T1>> GetPageExtended<T1, T2>(Expression<Func<T1, bool>> filter, Func<IQueryable<T2>, IOrderedQueryable<T2>> sorting, int page, int itemsPerPage, Func<IQueryable<T1>, IQueryable<T2>> grouping = null) 
            where T2 : T1
        {
            return
                await repository.GetPageExtended<T1, T2>(filter, sorting, page, itemsPerPage, grouping);
        }

        public async Task SaveChanges()
        {
            await repository.SaveChanges();
        }

        public void Update<T>(bool full, params T[] viewModel)
        {
            var prep = findMap<T>();
            if (prep == null) repository.Update<T>(full, viewModel);
            else prep.Update(repository, context, full, viewModel);
        }

        public void UpdateKeys()
        {
            repository.UpdateKeys();
        }

        public void UpdateList<T>(bool full, IEnumerable<T> oldValues, IEnumerable<T> newValues)
        {
            var prep = findMap<T>();
            if (prep == null) repository.UpdateList<T>(full, oldValues, newValues);
            else prep.UpdateList(repository, context, full, oldValues, newValues);
        }
    }
}
