using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class ObjectCopier<M,T>
    {
        List<KeyValuePair<PropertyInfo, PropertyInfo>> allProps = new List<KeyValuePair<PropertyInfo, PropertyInfo>>();
        public ObjectCopier(string noCopyPropertyName=null)
        {
            var tInfo = typeof(T).GetTypeInfo();
            var props=typeof(M).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.GetProperty);
            foreach(var prop in props)
            {
                if (prop.Name == noCopyPropertyName) continue;
                var tProp = tInfo.GetProperty(prop.Name, BindingFlags.Public | BindingFlags.SetProperty);
                if (tProp != null) allProps.Add(new KeyValuePair<PropertyInfo, PropertyInfo>(prop, tProp));
            }
        }
        public void Copy(M origin, T target)
        {
            foreach (var pair in allProps)
                pair.Value.SetValue(target, pair.Key.GetValue(origin));

        }
    }
}
