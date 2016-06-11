using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MvcControlsToolkit.Core.Linq;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class ChangeSet
    {
        public static ChangeSet<T, K>   Create<T, K> (IEnumerable<T> oldValues, IEnumerable<T> newValues, Expression<Func<T, K>> keyExpression)
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
                    foreach(var item in newValues)
                    {
                        var key = keyFunc(item);
                        if (key != null && dict.ContainsKey(key))
                        {
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

        

        public async Task UpdateDatabase<M>(DbSet<M> table, DbContext ctx,  bool saveChanges=false, bool retrieveChanged=true)
            where M: class, new()
        {

            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (table == null) throw new ArgumentNullException(nameof(table));
            var keyPropName = ExpressionHelper.GetExpressionText(KeyExpression);
            var keyProperty = typeof(M).GetTypeInfo().GetProperty(keyPropName);
            bool aChange=false;
            var copier = new ObjectCopier<T, M>(keyPropName);
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
                }
            }
            if (Changed != null && Changed.Count > 0)
            {

                if (retrieveChanged)
                {
                    var keyFunc = KeyExpression.Compile();
                    var ids=Changed.Select(m => keyFunc(m));
                    var filter=new FilterBuilder<M>()
                        .Add(FilterCondition.IsContainedIn, keyPropName, ids)
                        .Get();
                    
                    var toModify = await table.Where(filter).ToListAsync();
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
                    var keyFunc = KeyExpression.Compile();
                    foreach (var oItem in Inserted)
                    {
                        aChange = true;
                        var item = new M();
                        copier.Copy(oItem, item);
                        keyProperty.SetValue(item, keyFunc(oItem));
                        table.Attach(item);
                    }
                }
            }
            if (aChange && saveChanges) await ctx.SaveChangesAsync();
        }
    }

}
