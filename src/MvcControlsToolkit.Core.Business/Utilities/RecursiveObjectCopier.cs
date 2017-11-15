using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Linq;
using MvcControlsToolkit.Core.Linq.Internal;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    //public class RecursiveObjectCopier: IObjectCopier
    //{
    //    private Delegate transform;
    //    private ObjectChangesRegister changes;
    //    private static LambdaExpression buildEmptyLambda(Type sourceType, Type destinationType)
    //    {
    //        if (sourceType == null) throw new ArgumentException(nameof(sourceType));
    //        if (destinationType == null) throw new ArgumentException(nameof(destinationType));
    //        ParameterExpression par = Expression.Parameter(sourceType, "m");
    //        return Expression.Lambda(Expression.MemberInit(Expression.New(destinationType)), par);
    //    }
    //    public RecursiveObjectCopier(LambdaExpression specifications)
    //    {
    //        if (specifications == null) throw new ArgumentException(nameof(specifications));
    //        changes = new ObjectChangesRegister(null, true);
    //        specifications=ProjectionExpression<string>.BuildInternalExpression(specifications, null, changes);
    //        transform = specifications.Compile();
    //    }
    //    public RecursiveObjectCopier(Type sourceType, Type destinationType)
    //        : this(buildEmptyLambda(sourceType, destinationType))
    //    {

    //    }
    //    public object Copy(object source, object destination)
    //    {
    //        if (source == null) throw new ArgumentException(nameof(source));
    //        if (destination == null) throw new ArgumentException(nameof(destination));
    //        var newObj = transform.DynamicInvoke(source);
    //        changes.CopyChanges(newObj, destination);
    //        return destination;
    //    }
    //}
    public class RecursiveObjectCopier<S, D>: IObjectCopier<S, D>, IComputeIncludes<D>, IObjectCopier<D>
        where D: class, new()
    {
        private ObjectChangesRegister changes;
        Func<S, D> transform;
        private List<string> paths;
        List<KeyValuePair<TypeInfo, IObjectCopier<D>>> variations = null;
        HashSet<string> simplifier = null;
        Expression<Func<S, D>> originalSpecifications;
        public RecursiveObjectCopier(Expression<Func<S, D>> specifications=null)
        {
            //if (specifications == null) throw new ArgumentException(nameof(specifications));
            changes = new ObjectChangesRegister(null, true);
            originalSpecifications = specifications;
            specifications =ProjectionExpression<S>.BuildExpression(specifications, null,changes);
            transform = specifications.Compile();
            paths = changes.ComputePaths();
        }
        private IObjectCopier<D> getSpecifiCopier(S source)
        {
            if (variations == null) return this;
            foreach (var pair in variations)
            {
                if (pair.Key.IsAssignableFrom(source.GetType())) return pair.Value;
            }
            return this;
        }
        public D Copy(S source, D destination)
        {
            if (source == null) throw new ArgumentException(nameof(source));
            //if (destination == null) throw new ArgumentException(nameof(destination));
            var copier = getSpecifiCopier(source);
            if (copier != this) return copier.Copy(source, destination);
            try
            {
                var newObj = transform(source);
                if (destination == null) return newObj;
                changes.CopyChanges(newObj, destination);
                return destination;
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }
        public D Copy(object source, D destination)
        {
            
            if (source == null) throw new ArgumentException(nameof(source));

            return Copy((S)source, destination);
        }
        public RecursiveObjectCopier<S, D> AddSubclass<SC>(Expression<Func<SC, D>> specifications = null)
            where SC: S
        {

            if (variations==null)
            {
                
                variations = new List<KeyValuePair<TypeInfo, IObjectCopier<D>>>();
                var newCopier = new RecursiveObjectCopier<SC, D>(specifications);
                variations.Add(new KeyValuePair<TypeInfo, IObjectCopier<D>>(typeof(SC).GetTypeInfo(), newCopier));
                if (newCopier.paths != null)
                {
                    if(simplifier == null) simplifier =
                            paths == null ? new HashSet<string>() : new HashSet<string>(paths);
                    simplifier.UnionWith(newCopier.paths);
                    
                    paths = null;
                }
                else paths = newCopier.paths;


            }
            return this;
        }
        private PropertyInfo findProperty(Expression node)
        {
            if (node.NodeType == ExpressionType.Conditional)
            {
                var cond = node as ConditionalExpression;
                return findProperty(cond.IfTrue) ?? findProperty(cond.IfFalse);
            }
            else if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
                return findProperty((node as UnaryExpression).Operand);
            else if (node.NodeType == ExpressionType.MemberAccess)
            {
                var pAccess = node as MemberExpression;
                return pAccess?.Member as PropertyInfo;
            }
            else return null;
        }
        public PropertyInfo GetMappedProperty(PropertyInfo x)
        {
            if (changes == null) return null;
            var infos = changes.Changes.Where(m => m.Property == x).FirstOrDefault();
            if (infos == null || infos.EnumType != null || infos.Changes != null) return null;
            return findProperty(infos.ValueExpression);
        }
        public Func<IQueryable<D>, IQueryable<D>> GetIncludes()
        {
            if (paths == null && simplifier != null) paths = simplifier.ToList();
            return x =>
            {
                if (paths == null) return x;
                foreach (var s in paths)
                    x = x.Include(s);
                return x;
            };
        }
    }
}
