using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class ObjectCopier<M,T>
    {
        List<KeyValuePair<PropertyInfo, PropertyInfo>> allProps = new List<KeyValuePair<PropertyInfo, PropertyInfo>>();
        Action<M, T> compiled;
        List<KeyValuePair<PropertyInfo, List<KeyValuePair<PropertyInfo, PropertyInfo>>>> allNestedProps = new List<KeyValuePair<PropertyInfo, List<KeyValuePair<PropertyInfo, PropertyInfo>>>>();
        public ObjectCopier(string noCopyPropertyName=null, bool compile=false, Type sourceType = null, Type destinationType = null)
        {
            sourceType = sourceType ?? typeof(M);
            destinationType = destinationType ?? typeof(T);
            var tInfo = destinationType.GetTypeInfo();
            var sInfo = sourceType.GetTypeInfo();
            var props= sInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            
            foreach (var prop in props)
            {
                if (prop.Name == noCopyPropertyName) continue;
                else if (prop.Name == noCopyPropertyName || (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && !typeof(IConvertible).IsAssignableFrom(prop.PropertyType))) continue;
                var tProp = tInfo.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                if (tProp != null && tProp.PropertyType.IsAssignableFrom(prop.PropertyType)) allProps.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(prop, tProp));
                
            }
            if(typeof(IUpdateConnections).IsAssignableFrom(sourceType)){
                TypeInfo infos;
                foreach (var tProp in tInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty))
                {
                    if ((infos = tProp.PropertyType.GetTypeInfo()).IsClass)
                    {
                        List<KeyValuePair<PropertyInfo, PropertyInfo>> entry = null;
                        foreach (var nestedTProp in infos.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
                        {
                            if ((typeof(IEnumerable).IsAssignableFrom(nestedTProp.PropertyType) && !typeof(IConvertible).IsAssignableFrom(nestedTProp.PropertyType))) continue;
                            var nestedProp = sInfo.GetProperty(tProp.Name + nestedTProp.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                            if (nestedProp != null && nestedTProp.PropertyType.IsAssignableFrom(nestedProp.PropertyType))
                            {
                                if (entry == null) entry = new List<KeyValuePair<PropertyInfo, PropertyInfo>>();
                                entry.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(nestedTProp, nestedProp));
                            }
                        }
                        if (entry != null)
                        {
                            allNestedProps.Add(new KeyValuePair<PropertyInfo, List<KeyValuePair<PropertyInfo, PropertyInfo>>>(tProp, entry));
                        }
                    }
                }
            }
            if (compile)
            {
                compiled = buildExpression(sourceType, destinationType).Compile();
            }
        }
        public IEnumerable<PropertyInfo> GetNeededConnections(IUpdateConnections x)
        {
            if (x == null || allNestedProps==null) return null;
            return allNestedProps.Where(m => x.MayUpdate(m.Key.Name)).Select(m => m.Key);
        }
        
        public T Copy(M origin, T target)
        {
            if (compiled != null)
            {
                compiled(origin, target);
                return target;
            }
            else
            {
                foreach (var pair in allProps)
                    pair.Value.SetValue(target, pair.Key.GetValue(origin));
                foreach(var nested in allNestedProps)
                {
                    if (!(origin as IUpdateConnections).MayUpdate(nested.Key.PropertyType.Name)) continue;
                    var nestedOb = nested.Key.GetValue(target);
                    if (nestedOb == null)
                    {
                        nestedOb = Activator.CreateInstance(nested.Key.PropertyType);
                        nested.Key.SetValue(target, nestedOb);
                    }
                    foreach(var pair in nested.Value)
                    {
                        pair.Key.SetValue(nestedOb, pair.Value.GetValue(origin));
                    }
                }
                return target;
            }

        }
        private Expression<Action<M, T>> buildExpression(Type sourceType = null, Type destinationType = null)
        {
            var refVerify=typeof(IUpdateConnections).GetMethod("MayUpdate", new Type[] {typeof(string) });
            sourceType = sourceType ?? typeof(M);
            destinationType = destinationType ?? typeof(T);
            var parX = Expression.Parameter(typeof(M), "x");
            var parY = Expression.Parameter(typeof(T), "y");
            var convParX = sourceType == null || sourceType == typeof(M) ? parX :
                Expression.Convert(parX, sourceType) as Expression;
            var convParY = destinationType == null || destinationType == typeof(T) ? parY :
                Expression.Convert(parY, destinationType) as Expression;
            List<Expression> assignements = new List<Expression>();
            foreach (var pair in allProps)
            {
                var left = Expression.MakeMemberAccess(convParY, pair.Value);
                var right = Expression.MakeMemberAccess(convParX, pair.Key);
                assignements.Add(Expression.Assign(left, right)); 
            }
            foreach (var nested in allNestedProps)
            {
                List<Expression> innerBlock = new List<Expression>();
                var test = Expression.Equal(Expression.MakeMemberAccess(convParY, nested.Key),
                    Expression.Constant(null)
                    );
                var cond = Expression.IfThen(test, Expression.Assign(Expression.MakeMemberAccess(convParY, nested.Key), Expression.New(nested.Key.PropertyType)));
                innerBlock.Add(cond);
                foreach (var pair in nested.Value)
                {
                    var left = Expression.MakeMemberAccess(Expression.MakeMemberAccess(convParY, nested.Key), pair.Key);
                    var right = Expression.MakeMemberAccess(convParX, pair.Value);
                    innerBlock.Add(Expression.Assign(left, right));
                }
                //verify if inner block must be executed
                ;
                var call=Expression.Call(Expression.Convert(parX, typeof(IUpdateConnections)),
                    refVerify,
                    Expression.Constant(nested.Key.Name, typeof(string))
                    );
                var innerCond =
                    Expression.IfThen(call, Expression.Block(innerBlock));
                assignements.Add(innerCond);
            }
            return Expression.Lambda<Action<M, T>>(
                Expression.Block(assignements),
                new ParameterExpression[] { parX, parY });
        }
    }
}
