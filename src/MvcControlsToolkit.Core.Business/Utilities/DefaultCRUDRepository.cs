using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcControlsToolkit.Core.Business.Utilities.Internal;
using MvcControlsToolkit.Core.Linq;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    internal enum Operation { I, U, D, L };
    internal static class CRUDRepositoryHelper
    {
        private static Expression<Func<TSource, TProp>> GetPropertyExpression<TSource, TProp>(PropertyInfo prop)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TSource), "m");
            Expression body = Expression.MakeMemberAccess(parameter, prop);
            return Expression.Lambda(body, parameter) as Expression<Func<TSource, TProp>>;
        }
        internal static Func<Operation, bool, object, object, ChangeSet> ChangeSetBuilder<VM, VK>(PropertyInfo prop)
        {
            var keyExpression = GetPropertyExpression<VM, VK>(prop);
            return (o, full, x, y) =>
            {
                
                if (o == Operation.L)
                {
                    return ChangeSet.Create<VM, VK>(x as IEnumerable<VM>, y as IEnumerable<VM>, keyExpression, true);
                }
                else
                {
                    var res = new ChangeSet<VM, VK>();
                    res.KeyExpression = keyExpression;
                    if (o == Operation.D)
                    {
                        IList<VK> conv = new List<VK>();
                        foreach (var el in x as IEnumerable)
                            conv.Add((VK)el);
                        res.Deleted = conv;
                    }
                    else if (o == Operation.I)
                    {
                        res.Inserted = x as ICollection<VM>;
                    }
                    else res.Changed = x as ICollection<VM>;
                    return res;
                }
            };

        }
    }
    
    public class DefaultCRUDRepository
    {
        public static DefaultCRUDRepository<D, T> Create<D, T>(D dbContext, DbSet<T> table, Expression<Func<T, bool>> accessFilter = null, Expression<Func<T, bool>> selectFilter = null)
            where T : class, new()
            where D : DbContext
        {
            return new DefaultCRUDRepository<D, T>(dbContext, table, accessFilter, selectFilter);
        }
    }
    public class DefaultCRUDRepository<D,T>: DefaultCRUDRepository, ICRUDRepository
        where T: class, new()
        where D: DbContext
         
    {
        private static readonly ConcurrentDictionary<Type, Func<Operation, bool, object, object, ChangeSet>> CreatorCache = new ConcurrentDictionary<Type, Func<Operation, bool, object, object, ChangeSet>>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> KeyProperty = new ConcurrentDictionary<Type, PropertyInfo>();
        private static readonly ConcurrentDictionary<Type, object> Projections = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, Tuple<object, Func<IQueryable, IEnumerable>>> FilterProjections = new ConcurrentDictionary<Type, Tuple<object, Func<IQueryable, IEnumerable>>>();
        private static readonly ConcurrentDictionary<Type, object> CompiledProjections = new ConcurrentDictionary<Type, object>();
        public D Context { get; private set; }
        public DbSet<T> Table { get; private set; }

        public Expression<Func<T, bool>> AccessFilter { get; private set; }
        public Expression<Func<T, bool>> SelectFilter { get; private set; }
        private ChangeSet lastChangeSet;
        public DefaultCRUDRepository(D dbContext, DbSet<T> table, Expression<Func<T, bool>> accessFilter=null, Expression<Func<T, bool>> selectFilter = null)
        {
            Context = dbContext;
            Table = table;
            AccessFilter = accessFilter;
            SelectFilter = selectFilter;
        }

        public static void DeclareProjection<K>(Expression<Func<T, K>> proj)
        {
            if (proj == null) return;
            Projections[typeof(K)] = ProjectionExpression<T>.BuildExpression(proj, typeof(K).GetTypeInfo().IsInterface ? typeof(K) : null);
            
        }
        public static RecursiveObjectCopier<K, T> DeclareUpdateProjection<K>(Expression<Func<K, T>> proj)
        {
            if (proj == null) return null;
            return RecursiveCopiersCache.DeclareCopierSpecifications<K, T>(proj);

        }
        public static void DeclareQueryProjection<K, PK>(Expression<Func<T, K>> proj, Expression<Func<K, PK>> key)
        {
            if (proj == null || key == null) return;
            Func<IQueryable, IEnumerable> keys = (x) => (x as IQueryable<K>).Select(key).ToArray();
            FilterProjections[typeof(K)] = Tuple.
                Create<object, Func<IQueryable, IEnumerable>>(ProjectionExpression<T>.BuildExpression(proj, typeof(K).GetTypeInfo().IsInterface ? typeof(K) : null),
                keys);

        }
        public static Func<T, K> GetCompiledExpression<K>()
        {
            object res;
            if (CompiledProjections.TryGetValue(typeof(K), out res))
            {
                return res as Func<T, K>;
            }
            object pres=null;
            Projections.TryGetValue(typeof(K), out pres);
            Func<T, K> fres;
            if (pres != null) fres = (pres as Expression<Func<T, K>>).Compile();
            else fres=ProjectionExpression<T>.BuildExpression<K>(pres as Expression<Func<T, K>>).Compile();
            CompiledProjections[typeof(K)] = fres;
            return fres;
        }
        public static Expression<Func<T, K>> GetExpression<K>()
        {
            
            object pres = null;
            Projections.TryGetValue(typeof(K), out pres);
            return pres as Expression<Func<T, K>>;
        }
        public static Expression<Func<T, K>> GetQueryExpression<K>(out Func<IQueryable,IEnumerable> getKeys)
        {
            Tuple<object, Func<IQueryable,IEnumerable>> pres = null;
            FilterProjections.TryGetValue(typeof(K), out pres);
            getKeys = null;
            if (pres == null) return null;
            getKeys = pres.Item2;
            return pres.Item1 as Expression<Func<T, K>>;
        }


        private IEnumerable<string> GetKeyNames() 
        {
            var keys = Context.Model.FindEntityType(typeof(T))
                .GetKeys();
            var principalKey=keys.Where(m => m.Properties.All(l => l.IsPrimaryKey())).SingleOrDefault();
            if(principalKey != null)
            {
                return principalKey.Properties.Select(m => m.Name); 
            }
            return null;
        }
        private IEnumerable<PropertyInfo> GetKeyProperties()
        {
            var keys = Context.Model.FindEntityType(typeof(T))
                .GetKeys();
            var principalKey = keys.Where(m => m.Properties.All(l => l.IsPrimaryKey())).SingleOrDefault();
            if (principalKey != null)
            {
                return principalKey.Properties.Select(m => m.PropertyInfo);
            }
            return null;
        }
        private PropertyInfo GetKeyProperty<K>()
        {
            PropertyInfo res;
            var pair = typeof(K);
            if (KeyProperty.TryGetValue(pair, out res))
                return res;
            var keys = GetKeyProperties();
            if (keys.Count() > 1) throw new NotSupportedException(string.Format(Resources.UnsupportedMultipleKeys, typeof(T).Name));
            var key = keys.SingleOrDefault();
            var copier=RecursiveCopiersCache.Get<K, T>();
            if(copier != null)
            {
                return (copier as RecursiveObjectCopier<K, T>)?.GetMappedProperty(key);
            }
            return KeyProperty[pair]=typeof(K).GetTypeInfo().GetProperty(key.Name);
        }
        
        protected Expression<Func<T, bool>> BuildKeyFilter(object keyVal)
        {
            var prop = GetKeyProperty<T>();
            var parameter = Expression.Parameter(typeof(T), "m");
            var acc = Expression.MakeMemberAccess(parameter, prop);
            var comp = Expression.Equal(acc, Expression.Constant(keyVal));
            return Expression.Lambda<Func<T, bool>>(comp, parameter);
        }
        protected Expression<Func<T, bool>> BuildKeysFilter(IEnumerable keyVals)
        {
            var prop = GetKeyProperty<T>();
            return new FilterBuilder<T>().Add(FilterCondition.IsContainedIn, prop.Name, keyVals).Get();
            
        }

        private ChangeSet GetChangeset<VM>(Operation o, bool full, object x, object y)
        {
            Func<Operation, bool, object, object, ChangeSet> res;
            
            var pair = typeof(VM);
            if (CreatorCache.TryGetValue(pair, out res))
            {
                return res(o, full, x, y);
            }
            var prop = GetKeyProperty<VM>();
            var method = typeof(CRUDRepositoryHelper).GetTypeInfo().GetMethod("ChangeSetBuilder", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .MakeGenericMethod(new Type[] { typeof(VM), prop.PropertyType });
            res = method.Invoke(null, new object[] { prop })
                as Func<Operation, bool, object, object, ChangeSet>;
            CreatorCache[pair] = res;
            return res(o, full, x, y);
        }
        public virtual void Update<T1>(bool full , params T1[] viewModel)
        {
            var cs = GetChangeset<T1>(Operation.U, full, viewModel, null);
            var res=cs.UpdateDatabase(Table, Context, AccessFilter, false, !full);
            res.Wait();
            lastChangeSet = cs;
            
        }

        public virtual void UpdateList<T1>(bool full, IEnumerable < T1> oldValues, IEnumerable<T1> newValues)
        {
            var cs = GetChangeset<T1>(Operation.L, full, oldValues, newValues);
            var res=cs.UpdateDatabase(Table, Context, AccessFilter, false, !full);
            res.Wait();
            lastChangeSet = cs;
            
        }

        public virtual void Add<T1>(bool full, params T1[] viewModel)
        {
            var cs = GetChangeset<T1>(Operation.I, full, viewModel, null);
            var res =cs.UpdateDatabase(Table, Context, AccessFilter, false, !full);
            res.Wait();
            lastChangeSet = cs;
        }
        public virtual Func<object, object> GetKey { get { return lastChangeSet.GetKey; } }
        public virtual void Delete<U>(params U[] key)
        {
            var cs = GetChangeset<T>(Operation.D, false, key, null);
            var res = cs.UpdateDatabase(Table, Context, AccessFilter, false, false);
            res.Wait();
        }

        public async Task SaveChanges()
        {
            await Context.SaveChangesAsync();
            UpdateKeys();
            
        }

        public void UpdateKeys()
        {
            if(lastChangeSet != null && lastChangeSet.UpdateKeys != null)
            {
                lastChangeSet.UpdateKeys();
                
            }
        }

        public virtual async Task<T1> GetById<T1, U>(U key)
        {
            object proj;
            var pres = Table.Where(BuildKeyFilter(key));
            if (SelectFilter != null) pres = pres.Where(SelectFilter);
            if (Projections.TryGetValue(typeof(T1), out proj))
            {
                return await pres
                .Select(proj as Expression<Func<T, T1>>).SingleOrDefaultAsync();
            }
            else
                return await pres
                .Project().To<T1>().SingleOrDefaultAsync();
        }

        private IQueryable<T2> InternalGetPageExtended<T1, T2>(
            Expression<Func<T1, bool>> filter,
            Func<IQueryable<T2>, IOrderedQueryable<T2>> sorting,
            int page,
            int itemsPerPage,
            Func<IQueryable<T1>, IQueryable<T2>> grouping ,
            out Tuple<object, Func<IQueryable, IEnumerable>> fQueryProj
            )
        {
            if (sorting == null) throw new ArgumentNullException(nameof(sorting));
            
            IQueryable<T> start;
            if (SelectFilter != null) start = Table.Where(SelectFilter);
            else start = Table.Select(m => m);

            IQueryable<T1> proj=null;
            object projExp=null;
            fQueryProj = null;
            if(FilterProjections.TryGetValue(typeof(T1), out fQueryProj))
            {
                proj = start.Select(fQueryProj.Item1 as Expression<Func<T, T1>>);
            }
            else if (Projections.TryGetValue(typeof(T1), out projExp))
                proj = start.Select(projExp as Expression<Func<T, T1>>);
            else
                proj = start.Project().To<T1>();
            if (filter != null) proj = proj.Where(filter);
            IQueryable<T2> toGroup;
            if (grouping != null)
            {
                fQueryProj = null;
                toGroup = grouping(proj);
            }
            else
            {

                toGroup = proj as IQueryable<T2>;
            }
            if (toGroup == null) toGroup = proj.Project().To<T2>();
            return toGroup;
        }

        public virtual async Task<DataPage<T1>> GetPageExtended<T1, T2>(
            Expression<Func<T1, bool>> filter, 
            Func<IQueryable<T2>, IOrderedQueryable<T2>> sorting, 
            int page, 
            int itemsPerPage,
            Func<IQueryable<T1>, IQueryable<T2>> grouping=null
            )
            where T2: T1
        {
            page = page - 1;
            if (page < 0) page = 0;
            Tuple<object, Func<IQueryable, IEnumerable>> fQueryProj;
            var toGroup = InternalGetPageExtended<T1, T2>(
                filter, sorting, page, itemsPerPage, grouping,
                out fQueryProj
                );
            var res = new DataPage<T1>
            {
                TotalCount=await toGroup.CountAsync(),
                ItemsPerPage=itemsPerPage,
                Page = page+1
            };
            res.TotalPages = res.TotalCount / itemsPerPage;
            if (res.TotalCount % itemsPerPage > 0) res.TotalPages++;
            var sorted = sorting(toGroup);
            if (page > 0) toGroup = sorted.Skip(page* itemsPerPage).Take(itemsPerPage);
            else toGroup = sorted.Take(itemsPerPage);
            if (fQueryProj != null)
            {
                var allIds = typeof(T1) == typeof(T2) ? fQueryProj.Item2(toGroup) : fQueryProj.Item2(toGroup.Project().To<T1>());
                var toStart = Table.Where(BuildKeysFilter(allIds));
                object projExp = null;
                IQueryable<T1> proj = null;
                if (Projections.TryGetValue(typeof(T1), out projExp))
                    proj = toStart.Select(projExp as Expression<Func<T, T1>>);
                else
                    proj = toStart.Project().To<T1>();
                toGroup = proj as IQueryable<T2>;
                if (toGroup == null) toGroup = proj.Project().To<T2>();
                toGroup = sorting(toGroup);
        }
            if (typeof(T1) == typeof(T2))
                res.Data = (await toGroup.ToArrayAsync()) as T1[];
            else if (grouping ==null)
                res.Data =  (await toGroup.Project().To<T1>().ToArrayAsync()) ;
            else 
                res.Data = (await toGroup.ToArrayAsync()).Select(m => (T1)m).ToArray();
            return res;
        }
        
        public virtual DataPage<T1> GetPageExtendedSync<T1, T2>(
            Expression<Func<T1, bool>> filter,
            Func<IQueryable<T2>, IOrderedQueryable<T2>> sorting,
            int page,
            int itemsPerPage,
            Func<IQueryable<T1>, IQueryable<T2>> grouping = null
            )
            where T2 : T1
        {
            page = page - 1;
            if (page < 0) page = 0;
             Tuple<object, Func<IQueryable, IEnumerable>> fQueryProj;
            var toGroup = InternalGetPageExtended<T1, T2>(
                filter, sorting, page, itemsPerPage, grouping,
                out fQueryProj
                );
            var res = new DataPage<T1>
            {
                TotalCount = toGroup.Count(),
                ItemsPerPage = itemsPerPage,
                Page = page + 1
            };
            res.TotalPages = res.TotalCount / itemsPerPage;
            if (res.TotalCount % itemsPerPage > 0) res.TotalPages++;
            var sorted = sorting(toGroup);
            if (page > 0) toGroup = sorted.Skip(page * itemsPerPage).Take(itemsPerPage);
            else toGroup = sorted.Take(itemsPerPage);
            if (fQueryProj != null)
            {
                var allIds = typeof(T1) == typeof(T2) ? fQueryProj.Item2(toGroup) : fQueryProj.Item2(toGroup.Project().To<T1>());
                var toStart = Table.Where(BuildKeysFilter(allIds));
                object projExp = null;
                IQueryable<T1> proj = null;
                if (Projections.TryGetValue(typeof(T1), out projExp))
                    proj = toStart.Select(projExp as Expression<Func<T, T1>>);
                else
                    proj = toStart.Project().To<T1>();
                toGroup = proj as IQueryable<T2>;
                if (toGroup == null) toGroup = proj.Project().To<T2>();
                toGroup = sorting(toGroup);
            }
            if (typeof(T1) == typeof(T2))
                res.Data = toGroup.ToArray() as T1[];
            else if (grouping == null)
                res.Data = toGroup.Project().To<T1>().ToArray();
            else
                res.Data = toGroup.ToArray().Select(m => (T1)m).ToArray();
            return res;
        }
        public virtual async Task<DataPage<T1>> GetPage<T1>(
            Expression<Func<T1, bool>> filter,
            Func<IQueryable<T1>, IOrderedQueryable<T1>> sorting,
            int page,
            int itemsPerPage,
            Func<IQueryable<T1>, IQueryable<T1>> grouping = null
            )
        {

            return await GetPageExtended<T1, T1>(filter, sorting, page, itemsPerPage, grouping);
        }
        public DataPage<T1> GetPageSync<T1>(
            Expression<Func<T1, bool>> filter,
            Func<IQueryable<T1>, IOrderedQueryable<T1>> sorting,
            int page,
            int itemsPerPage,
            Func<IQueryable<T1>, IQueryable<T1>> grouping = null
            )
        {

            return GetPageExtendedSync<T1, T1>(filter, sorting, page, itemsPerPage, grouping);
        }
    }
}
