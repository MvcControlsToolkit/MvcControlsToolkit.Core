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
            var tInfo = (destinationType ?? typeof(T)).GetTypeInfo();
            var sInfo = (sourceType ?? typeof(M)).GetTypeInfo();
            var props= sInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            
            foreach (var prop in props)
            {
                if (prop.Name == noCopyPropertyName) continue;
                else if (prop.Name == noCopyPropertyName || (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && !typeof(IConvertible).IsAssignableFrom(prop.PropertyType))) continue;
                var tProp = tInfo.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                if (tProp != null && tProp.PropertyType.IsAssignableFrom(prop.PropertyType)) allProps.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(prop, tProp));
                
            }
            TypeInfo infos;
            foreach (var tProp in tInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty))
            {
                if ((infos = tProp.PropertyType.GetTypeInfo()).IsClass)
                {
                    List<KeyValuePair<PropertyInfo, PropertyInfo>> entry=null;
                    foreach (var nestedTProp in infos.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty))
                    {
                        if ((typeof(IEnumerable).IsAssignableFrom(nestedTProp.PropertyType) && !typeof(IConvertible).IsAssignableFrom(nestedTProp.PropertyType))) continue;
                        var nestedProp = sInfo.GetProperty(tProp.Name + nestedTProp.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                        if (nestedProp != null && nestedTProp.PropertyType.IsAssignableFrom(nestedProp.PropertyType))
                        {
                            if (entry == null) entry = new List<KeyValuePair<PropertyInfo, PropertyInfo>>();
                            entry.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(nestedTProp, nestedProp));
                        }
                    }
                    if(entry!= null)
                    {
                        allNestedProps.Add(new KeyValuePair<PropertyInfo, List<KeyValuePair<PropertyInfo, PropertyInfo>>>(tProp, entry));
                    }
                }
            }
            if (compile)
            {
                compiled = buildExpression().Compile();
            }
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
            var parX = Expression.Parameter(sourceType??typeof(M), "x");
            var parY = Expression.Parameter(destinationType??typeof(T), "y");
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
                var test = Expression.Equal(Expression.MakeMemberAccess(convParY, nested.Key),
                    Expression.Constant(null)
                    );
                var cond = Expression.Condition(test, Expression.New(nested.Key.PropertyType), Expression.Constant(null));
                assignements.Add(cond);
                foreach (var pair in nested.Value)
                {
                    var left = Expression.MakeMemberAccess(Expression.MakeMemberAccess(convParY, nested.Key), pair.Key);
                    var right = Expression.MakeMemberAccess(convParX, pair.Value);
                    assignements.Add(Expression.Assign(left, right));
                }
            }
            return Expression.Lambda<Action<M, T>>(
                Expression.Block(assignements),
                new ParameterExpression[] { parX, parY });
        }
    }
}
