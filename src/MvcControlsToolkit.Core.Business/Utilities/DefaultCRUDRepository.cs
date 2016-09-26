using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
                        res.Deleted = x as ICollection<VK>;
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
        public D Context { get; private set; }
        public DbSet<T> Table { get; private set; }

        public Expression<Func<T, bool>> AccessFilter { get; private set; }
        public Expression<Func<T, bool>> SelectFilter { get; private set; }
        private string keyName;
        private ChangeSet lastChangeSet;
        internal DefaultCRUDRepository(D dbContext, DbSet<T> table, Expression<Func<T, bool>> accessFilter=null, Expression<Func<T, bool>> selectFilter = null)
        {
            Context = dbContext;
            Table = table;
            AccessFilter = accessFilter;
            SelectFilter = selectFilter;
        }

        public static void DeclareProjection<K>(Expression<Func<T, K>> proj)
        {
            Projections[typeof(K)] = proj;
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
        private PropertyInfo GetKeyProperty<K>()
        {
            PropertyInfo res;
            var pair = typeof(K);
            if (KeyProperty.TryGetValue(pair, out res))
                return res;
            var keys = GetKeyNames();
            if (keys.Count() > 1) throw new NotSupportedException(string.Format(Resources.UnsupportedMultipleKeys, typeof(T).Name));
            var key = keys.SingleOrDefault();
            return KeyProperty[pair]=typeof(K).GetTypeInfo().GetProperty(key);
        }
        
        protected Expression<Func<T, bool>> BuildKeyFilter(object keyVal)
        {
            var prop = GetKeyProperty<T>();
            var parameter = Expression.Parameter(typeof(T), "m");
            var acc = Expression.MakeMemberAccess(parameter, prop);
            var comp = Expression.Equal(acc, Expression.Constant(keyVal));
            return Expression.Lambda<Func<T, bool>>(comp, parameter);
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
                .Project().To<T1>(proj as Expression<Func<T, T1>>).SingleOrDefaultAsync();
            }
            else
                return await pres
                .Project().To<T1>().SingleOrDefaultAsync();
        }

        public virtual async Task<DataPage<T1>> GetPage<T1>(
            Expression<Func<T1, bool>> filter, 
            Func<IQueryable<T1>, IOrderedQueryable<T1>> sorting, 
            int page, 
            int itemsPerPage)
        {

            if (sorting == null) throw new ArgumentNullException(nameof(sorting));
            page = page - 1;
            if (page < 0) page = 0;
            IQueryable<T> start;
            if (SelectFilter != null) start = Table.Where(SelectFilter);
            else start = Table.Select(m => m);

            IQueryable<T1> proj;
            object projExp;
            if (Projections.TryGetValue(typeof(T1), out projExp))
                proj = start.Project().To<T1>(projExp as Expression<Func<T, T1>>);
            else
                proj = start.Project().To<T1>();
            if (filter != null) proj = proj.Where(filter);

            var res = new DataPage<T1>
            {
                TotalCount=await proj.CountAsync(),
                ItemsPerPage=itemsPerPage,
                Page = page+1
            };
            res.TotalPages = res.TotalCount / itemsPerPage;
            if (res.TotalCount % itemsPerPage > 0) res.TotalPages++;
            var sorted = sorting(proj);
            if (page > 0) proj = sorted.Skip(page* itemsPerPage).Take(itemsPerPage);
            else proj = sorted.Take(itemsPerPage);
            res.Data = await proj.ToArrayAsync();
            return res;
        }
    }
}
