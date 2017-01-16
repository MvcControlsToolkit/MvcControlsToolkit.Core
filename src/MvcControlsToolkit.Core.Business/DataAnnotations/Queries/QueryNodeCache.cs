using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace MvcControlsToolkit.Core.DataAnnotations.Queries
{
    public static class QueryNodeCache
    {
        private static ConcurrentDictionary<Tuple<Type, string>, Tuple<IList<PropertyInfo>, QueryAttribute>>
            paths = new ConcurrentDictionary<Tuple<Type, string>, Tuple<IList<PropertyInfo>, QueryAttribute>>();

        private static ConcurrentDictionary<Tuple<Type, string>, MethodInfo>
            methods = new ConcurrentDictionary<Tuple<Type, string>, MethodInfo>();

        private static ConcurrentDictionary<Type, bool>
            searchEnabled = new ConcurrentDictionary<Type, bool>();

        public static Tuple<IList<PropertyInfo>, QueryAttribute> GetPath(Type t, string name)
        {
            Tuple<IList<PropertyInfo>, QueryAttribute> result;
            var search = new Tuple<Type, string>(t, name);
            if (paths.TryGetValue(search, out result))
            {
                return result;
            }
            string[] names = name.Split('.');
            List<PropertyInfo> properties = new List<PropertyInfo>();
            Type currType = t;
            PropertyInfo lastProp = null;
            int i = 0;
            foreach (var property in names)
            {
                lastProp = t.GetTypeInfo().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                i++;
                if (lastProp == null) throw new WrongPropertyNameException(string.Join(".", 0, i));
                properties.Add(lastProp);
                currType = lastProp.PropertyType;
            }
            QueryAttribute attribute = null;
            if(lastProp != null)
            {
                attribute = lastProp.GetCustomAttribute(typeof(QueryAttribute)) as QueryAttribute;
            }
            var res = new Tuple<IList<PropertyInfo>, QueryAttribute>(properties, attribute);
            paths.TryAdd(search, res);
            return res;
        }

        public static MethodInfo GetMethod(Type t, string name, Type argType)
        {
            var search = new Tuple<Type, string>(t, name);
            MethodInfo result;
            if (methods.TryGetValue(search, out result))
            {
                return result;
            }
            var method = argType == null ? t.GetTypeInfo().GetMethod(name, new Type[0]) : t.GetTypeInfo().GetMethod(name, new Type[] { argType });
            if (method == null) new OperationNotAllowedException(null, name);
            methods.TryAdd(search, method);
            return method;
        }
        public static bool GetSearchEnabled(Type t)
        {
            bool result;
            if (searchEnabled.TryGetValue(t, out result))
            {
                return result;
            }
            result = t.GetTypeInfo().GetCustomAttribute(typeof(QuerySearchAttribute)) != null;
            searchEnabled.TryAdd(t, result);
            return result;
        }
    }
}
