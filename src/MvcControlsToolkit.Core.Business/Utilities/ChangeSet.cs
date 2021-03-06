﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MvcControlsToolkit.Core.Linq;
using System.Collections;
using System.Collections.Concurrent;
using MvcControlsToolkit.Core.Business.Utilities.Internal;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public abstract class ChangeSet
    {
        protected  static readonly ConcurrentDictionary<KeyValuePair<Type, Type>, object> CopierCache = new ConcurrentDictionary<KeyValuePair<Type, Type>, object>();
        protected static readonly ConcurrentDictionary<Type, PropertyInfo> KeyProperties = new ConcurrentDictionary<Type,PropertyInfo>();
        protected static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> AllProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        protected static readonly ConcurrentDictionary<Type, object> CompiledKeys = new ConcurrentDictionary<Type, object>();
        protected static IObjectCopier<T, M>  GetStandardCopier<T, M>(Type sourceType=null, Type destinationType=null)
            where M: new()
        {
            object res = null;
            var pair = new KeyValuePair<Type, Type>(sourceType??typeof(T), destinationType??typeof(M));
            if (CopierCache.TryGetValue(pair, out res))
                return (IObjectCopier<T, M>)res;
            else return (IObjectCopier<T, M>)(CopierCache[pair] = new ObjectCopier<T, M>(null, true, sourceType, destinationType));
        }
        Type lastCopierSource=null;
        Type lastCopierDestination = null;
        object lastCopier = null;
        protected IObjectCopier<T, M> GetCopierOptimized<T, M>(Type sourceType = null, Type destinationType = null)
            where M : new()
        {
            sourceType = typeof(T).GetTypeInfo().IsInterface ? typeof(T) : (sourceType ?? typeof(T));
            destinationType = destinationType ?? typeof(M);
            if (sourceType == lastCopierSource && destinationType == lastCopierDestination)
                return lastCopier as IObjectCopier<T, M>;
            var res = GetStandardCopier<T, M>(sourceType, destinationType);
            lastCopier = res;
            return res;
        }
        protected static Func<T, K> GetCopiledKey<T, K>(Expression<Func<T, K>> keyExpression)
        {
            object res;
            if (CompiledKeys.TryGetValue(typeof(T), out res))
                return (Func<T, K>)res;
            else
            {
                res = keyExpression.Compile();
                CompiledKeys[typeof(T)] = res;
                return (Func<T, K>)res;
            }
            
        }
        protected static PropertyInfo GetKeyProperty<T, K>(Expression<Func<T, K>> keyExpression, Type m)
        {
            PropertyInfo res;
            if (KeyProperties.TryGetValue(m, out res))
                return res;
            else
            {
                res = m.GetTypeInfo().GetProperty(ExpressionHelper.GetExpressionText(keyExpression));
                KeyProperties[m] = res;
                return res;
            }

        }
        protected static IEnumerable<PropertyInfo> GetProperties(Type m)
        {
            IEnumerable<PropertyInfo> res;
            if (AllProperties.TryGetValue(m, out res))
                return res;
            else
            {
                res = m.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                            .Where(l => !typeof(IEnumerable).IsAssignableFrom(l.PropertyType) || typeof(IConvertible).IsAssignableFrom(l.PropertyType));
                AllProperties[m] = res;
                return res;
            }

        }
        private Type LastType;
        private IEnumerable<PropertyInfo> LastProperties;
        protected IEnumerable<PropertyInfo> GetPropertiesOptimized(Type m)
        {
            if (m == LastType) return LastProperties;
            LastType = m;
            return LastProperties = GetProperties(m);
        }
        private static bool changed(IEnumerable<PropertyInfo> props, object oldItem, object newItem)
        {
            foreach(var prop in props)
            {
                var old = prop.GetValue(oldItem);
                var newV = prop.GetValue(newItem);
                if (old == null)
                {
                    if (newV != null) return true;
                }
                else if (!old.Equals(newV)) return true;
                
            }
            return false;
        }

        public object BeforeAdd { get; set; }
        public object BeforeUpdate { get; set; }

        public static ChangeSet<T, K>   Create<T, K> (IEnumerable<T> oldValues, IEnumerable<T> newValues, Expression<Func<T, K>> keyExpression, bool verifyPropertyChanges=true)
        {
            bool supposeModify = Nullable.GetUnderlyingType(typeof(K)) != null;
            if (keyExpression == null) throw new ArgumentNullException(nameof(keyExpression));
            var res = new ChangeSet<T, K>();
            res.KeyExpression = keyExpression;
            var keyFunc = GetCopiledKey(keyExpression);
            if(oldValues == null)
            {
                res.Inserted = newValues.ToList();
            }
            else
            {
                IDictionary<K,T > dict = oldValues.ToDictionary(keyFunc);
                res.Inserted = new List<T>();
                res.Deleted = new List<K>();
                res.Changed = new List<T>();
                res.ChangedOldValues = new List<T>();
                if(newValues != null)
                {
                    IEnumerable<PropertyInfo> props = null;
                    bool isInterface = typeof(T).GetTypeInfo().IsInterface;
                    foreach (var item in newValues)
                    {
                        var key = keyFunc(item);
                        if (key != null && (supposeModify || dict.ContainsKey(key)))
                        {

                            T other = default(T);
                            dict.TryGetValue(key, out other);
                            if (other == null || !verifyPropertyChanges || changed(props = res.GetPropertiesOptimized(item == null || isInterface ? typeof(T) : item.GetType()), other, item))
                                res.Changed.Add(item);
                            if (other != null)
                            {
                                res.ChangedOldValues.Add(other);
                                dict.Remove(key);
                            }
                            else res.ChangedOldValues.Add(item);
                        }
                        else res.Inserted.Add(item);
                    }
                }
                res.Deleted=dict.Select(m => keyFunc(m.Value)).ToList();
            }
            return res;
             
        }
        public Action UpdateKeys { get; protected set; }
        public abstract  Task<List<M>> UpdateDatabase<M>(DbSet<M> table, DbContext ctx, Expression<Func<M, bool>> accessFilter = null, bool saveChanges = false, bool retrieveChanged = true)
            where M : class, new();
        public abstract object GetKey(object x);
    }
    public class ChangeSet<T,K>: ChangeSet
    {

        public Expression<Func<T, K>> KeyExpression { get; set; }
        public Func<T, K> CompiledKeyExpression { get { return GetCopiledKey(KeyExpression); } }
        public ICollection<T> Inserted { get; set; }
        public ICollection<T>  Changed { get; set; }
        public ICollection<T> ChangedOldValues { get; set; }
        public ICollection<K> Deleted { get; set; }
        
        public override object GetKey(object x)
        {

            if (x == null) return null;
            return CompiledKeyExpression((T)x);
        }

        public override async Task<List<M>> UpdateDatabase<M>(DbSet<M> table, DbContext ctx,  Expression<Func<M, bool>> accessFilter=null, bool saveChanges=false, bool retrieveChanged=true) 
        {

            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (table == null) throw new ArgumentNullException(nameof(table));
            var keyPropName = ExpressionHelper.GetExpressionText(KeyExpression);
            var keyProperty = GetKeyProperty(KeyExpression, typeof(M));
            var keyFunc = GetCopiledKey(KeyExpression);
            bool aChange=false;
            
            IEnumerable<K> changedIds = null;
            Expression<Func<M, bool>> changedFilter = null;
            Expression<Func<M, bool>> deletedFilter = null;
            IObjectCopier<T, M> recursiveCopier = RecursiveCopiersCache.Get<T, M>();
            IObjectCopier<T,M> copier = null ;
            if (!retrieveChanged && accessFilter != null && Deleted != null && Deleted.Count > 0)
            {
                var deletedIds = Deleted;
                deletedFilter = new FilterBuilder<M>()
                            .Add(FilterCondition.IsContainedIn, keyPropName, deletedIds)
                            .Get();
                if(await table.Where(deletedFilter).Where(accessFilter).CountAsync() !=
                    await table.Where(deletedFilter).CountAsync())
                    return null;

            }
            if (!retrieveChanged && accessFilter != null && Changed != null && Changed.Count > 0)
            {
                changedIds = Changed.Select(m => keyFunc(m));
                changedFilter = new FilterBuilder<M>()
                            .Add(FilterCondition.IsContainedIn, keyPropName, changedIds)
                            .Get();
                if (await table.Where(changedFilter).Where(accessFilter).CountAsync() !=
                    await table.Where(changedFilter).Where(accessFilter).CountAsync())
                    return null;
            }
            var res = new List<M>();
            if (Deleted != null && Deleted.Count > 0)
            {
                IEnumerable<object> toDelete;
                if (retrieveChanged)
                {
                    var DBParam = Expression.Parameter(typeof(M), "m");
                    var DBKeyExpression = Expression.Lambda(
                        Expression.TypeAs(
                            Expression.MakeMemberAccess(DBParam, keyProperty),
                            typeof(object)),
                        DBParam);
                    var deletedIds = Deleted;
                    if (deletedFilter == null)
                        deletedFilter = new FilterBuilder<M>()
                            .Add(FilterCondition.IsContainedIn, keyPropName, deletedIds)
                            .Get();
                    if (accessFilter == null)
                        toDelete = await table.Where(deletedFilter).Select((Expression<Func<M, object>>)DBKeyExpression)
                        .ToListAsync();
                    else
                        toDelete = await table.Where(deletedFilter).Where(accessFilter).Select((Expression<Func<M, object>>)DBKeyExpression)
                            .ToListAsync();

                }
                else toDelete = Deleted.Select(m => m as object);
                foreach (var key in Deleted)
                {
                    aChange = true;
                    var item = new M();
                    keyProperty.SetValue(item, key);
                    table.Attach(item);
                    ctx.Entry(item).State = EntityState.Deleted;
                }
                
            }
            if (Inserted != null && Inserted.Count > 0)
            {
                
                foreach (var oItem in Inserted)
                {
                    copier = recursiveCopier??GetCopierOptimized<T, M>(oItem.GetType());
                    aChange = true;
                    var item = copier.Copy(oItem, null);
                    if (BeforeAdd != null) await (BeforeAdd as Func<M, bool, object, Task>)(item, !retrieveChanged, oItem);
                    table.Add(item);
                    res.Add(item);
                }
            }
            if (Changed != null && Changed.Count > 0)
            {

                if (retrieveChanged)
                {

                    if (changedIds == null) changedIds = Changed.Select(m => keyFunc(m));
                    if(changedFilter == null)
                        changedFilter = new FilterBuilder<M>()
                            .Add(FilterCondition.IsContainedIn, keyPropName, changedIds)
                            .Get();
                    IEnumerable<PropertyInfo> connections=null;
                    if (recursiveCopier == null)
                    {
                        connections = Changed.SelectMany(m => (GetCopierOptimized<T, M>(m.GetType()) as IComputeConnections)?.GetNeededConnections(m as IUpdateConnections))
                            .Where(m => m != null)
                            .Distinct();
                    }
                    
                    var query = table.Where(changedFilter);
                    if (accessFilter != null) query = query.Where(accessFilter);
                    if (connections != null)
                    {
                        foreach(var conn in connections)
                        {
                            query = query.Include(conn.Name);
                        }
                    }
                    if(connections == null && recursiveCopier != null && recursiveCopier is IComputeIncludes<M>)
                    {
                        var fun = (recursiveCopier as IComputeIncludes<M>).GetIncludes();
                        if (fun != null) query = fun(query);
                    }
                    var toModify = await query.ToListAsync();
                    var dict = Changed.ToDictionary(keyFunc);
                    foreach(var item in toModify)
                    {
                        
                        aChange = true;
                        var key = keyProperty.GetValue(item);
                        var oItem = dict[(K)key];
                        copier = recursiveCopier??GetCopierOptimized<T, M>(oItem.GetType());
                        copier.Copy(oItem, item);
                        if (BeforeUpdate != null) await (BeforeUpdate as Func<M, bool, object, Task>)(item, !retrieveChanged, oItem);
                    }
                }
                else
                {
                    
                    foreach (var oItem in Changed)
                    {
                        aChange = true;
                        copier = recursiveCopier??GetCopierOptimized<T, M>(oItem.GetType());
                        var item = copier.Copy(oItem, null);
                        if (BeforeUpdate != null) await (BeforeUpdate as Func<M, bool, object, Task>)(item, !retrieveChanged, oItem);
                        table.Attach(item);
                    }
                }
            }
            UpdateKeys = () =>
             {
                 if (res.Count > 0)
                 {
                     int i = 0;
                     var keyOProperty = typeof(T).GetTypeInfo().GetProperty(keyPropName);
                     foreach (var oItem in Inserted)
                     {
                         var item = res[i];
                         keyOProperty.SetValue(oItem, keyProperty.GetValue(item));
                         i++;
                     }
                 }
                 UpdateKeys = null;
             };
            if (aChange && saveChanges)
            {
                await ctx.SaveChangesAsync();
                UpdateKeys();
            }
            return res;
        }
    }

}
