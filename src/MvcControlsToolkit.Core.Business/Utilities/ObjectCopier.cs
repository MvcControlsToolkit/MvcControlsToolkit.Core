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
        public ObjectCopier(string noCopyPropertyName=null, bool compile=false)
        {
            var tInfo = typeof(T).GetTypeInfo();
            var props=typeof(M).GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach(var prop in props)
            {
                if (prop.Name == noCopyPropertyName || (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && !typeof(IConvertible).IsAssignableFrom(prop.PropertyType))) continue;
                var tProp = tInfo.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                if (tProp != null && tProp.PropertyType.IsAssignableFrom(prop.PropertyType)) allProps.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(prop, tProp));
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
                return target;
            }

        }
        private Expression<Action<M, T>> buildExpression()
        {
            var parX = Expression.Parameter(typeof(M), "x");
            var parY = Expression.Parameter(typeof(T), "y");
            Expression[] assignements = new Expression[allProps.Count];
            int i = 0;
            foreach (var pair in allProps)
            {
                var left = Expression.MakeMemberAccess(parY, pair.Value);
                var right = Expression.MakeMemberAccess(parX, pair.Key);
                assignements[i] = Expression.Assign(left, right);
                i++;
            }
            return Expression.Lambda<Action<M, T>>(
                Expression.Block(assignements),
                new ParameterExpression[] { parX, parY });
        }
    }
}
