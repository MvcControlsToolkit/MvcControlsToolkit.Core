using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MvcControlsToolkit.Core.Linq;
using System.Collections;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class ChangeSet
    {
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
        public static ChangeSet<T, K>   Create<T, K> (IEnumerable<T> oldValues, IEnumerable<T> newValues, Expression<Func<T, K>> keyExpression, bool verifyPropertyChanges=true)
        {
            if (keyExpression == null) throw new ArgumentNullException(nameof(keyExpression));
            var res = new ChangeSet<T, K>();
            res.KeyExpression = keyExpression;
            var keyFunc = keyExpression.Compile();
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
                    if (verifyPropertyChanges)
                    {
                        props = typeof(T).GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                            .Where(m => !typeof(IEnumerable).IsAssignableFrom(m.PropertyType) || typeof(IConvertible).IsAssignableFrom(m.PropertyType));
                    }
                    foreach (var item in newValues)
                    {
                        var key = keyFunc(item);
                        if (key != null && dict.ContainsKey(key))
                        {
                            if(!verifyPropertyChanges || changed(props, dict[key], item))
                            res.Changed.Add(item);
                            res.ChangedOldValues.Add(dict[key]);
                            dict.Remove(key);
                        }
                        else res.Inserted.Add(item);
                    }
                }
                res.Deleted=dict.Select(m => keyFunc(m.Value)).ToList();
            }
            return res;
             
        }
    }
    public class ChangeSet<T,K>: ChangeSet
    {

        public Expression<Func<T, K>> KeyExpression { get; set; }
        public ICollection<T> Inserted { get; set; }
        public ICollection<T>  Changed { get; set; }
        public ICollection<T> ChangedOldValues { get; set; }
        public ICollection<K> Deleted { get; set; }

        

        public async Task<List<M>> UpdateDatabase<M>(DbSet<M> table, DbContext ctx,  Expression<Func<M, bool>> accessFilter=null, bool saveChanges=false, bool retrieveChanged=true)
            where M: class, new()
        {

            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (table == null) throw new ArgumentNullException(nameof(table));
            var keyPropName = ExpressionHelper.GetExpressionText(KeyExpression);
            var keyProperty = typeof(M).GetTypeInfo().GetProperty(keyPropName);
            var keyFunc = KeyExpression.Compile();
            bool aChange=false;
            IEnumerable<K> changedIds = null;
            Expression<Func<M, bool>> changedFilter = null;
            var copier = new ObjectCopier<T, M>(keyPropName);
            if (accessFilter != null && Deleted != null && Deleted.Count > 0)
            {
                var deletedIds = Deleted;
                var deletedFilter = new FilterBuilder<M>()
                            .Add(FilterCondition.IsContainedIn, keyPropName, deletedIds)
                            .Get();
                if(await table.Where(deletedFilter).Where(accessFilter).CountAsync() != Deleted.Count)
                    return null;

            }
            if (accessFilter != null && Changed != null && Changed.Count > 0)
            {
                changedIds = Changed.Select(m => keyFunc(m));
                changedFilter = new FilterBuilder<M>()
                            .Add(FilterCondition.IsContainedIn, keyPropName, changedIds)
                            .Get();
                if (await table.Where(changedFilter).Where(accessFilter).CountAsync() != Changed.Count)
                    return null;
            }
            var res = new List<M>();
            if (Deleted != null && Deleted.Count > 0)
            {
                foreach(var key in Deleted)
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
                    aChange = true;
                    var item = new M();
                    copier.Copy(oItem, item);
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
                    
                    var toModify = await table.Where(changedFilter).ToListAsync();
                    var dict = Changed.ToDictionary(keyFunc);
                    foreach(var item in toModify)
                    {
                        aChange = true;
                        var key = keyProperty.GetValue(item);
                        var oItem = dict[(K)key];
                        copier.Copy(oItem, item);
                    }
                }
                else
                {
                    
                    foreach (var oItem in Changed)
                    {
                        aChange = true;
                        var item = new M();
                        copier.Copy(oItem, item);
                        keyProperty.SetValue(item, keyFunc(oItem));
                        table.Attach(item);
                    }
                }
            }
            if (aChange && saveChanges)
            {
                await ctx.SaveChangesAsync();
                if(res.Count > 0)
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
            }
            return res;
        }
    }

}
