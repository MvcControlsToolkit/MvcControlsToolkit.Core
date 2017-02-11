using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MvcControlsToolkit.Core.DataAnnotations.Queries
{
    public static class QueryNodeCache
    {
        private static ConcurrentDictionary<Tuple<Type, string>, Tuple<IList<PropertyInfo>, QueryAttribute>>
            paths = new ConcurrentDictionary<Tuple<Type, string>, Tuple<IList<PropertyInfo>, QueryAttribute>>();

        private static ConcurrentDictionary<Tuple<string, Type>, MethodInfo>
            methods = new ConcurrentDictionary<Tuple<string, Type>, MethodInfo>();

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
                lastProp = currType.GetTypeInfo().GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                i++;
                if (lastProp == null) throw new WrongPropertyNameException(string.Join(".", names, 0, i));
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
            
            MethodInfo result;
            Type[] atypes = argType==null?null: argType.GetGenericArguments();
            Type aggType = argType == null ? null : atypes[1];
            var search = new Tuple<string, Type>(name, aggType);
            if (!methods.TryGetValue(search, out result))
            {
                result= argType == null ? typeof(Enumerable).GetTypeInfo().GetMethods().Where(m => m.Name == name && m.GetParameters().Count() == 1).FirstOrDefault()
                : (name == "Select" ?
                    typeof(Enumerable).GetTypeInfo().GetMethods().Where(m => m.Name == name && m.GetParameters().Count() == 2).FirstOrDefault()
                    : 
                    typeof(Enumerable).GetTypeInfo().GetMethods().Where(m => m.Name == name && m.GetParameters().Count() == 2 && (m.GetParameters()[1].ParameterType.GetGenericArguments()[1] == aggType)).FirstOrDefault());
                if (result == null) new OperationNotAllowedException(null, name);
                methods.TryAdd(search, result);
            }

            
            if (argType != null)
            {
                if(name == "Select")
                    result = result.MakeGenericMethod(atypes);
                else
                    result = result.MakeGenericMethod(atypes[0]);
            }
            else result = result.MakeGenericMethod(t);
            return result;
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
