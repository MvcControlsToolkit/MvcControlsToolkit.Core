using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;

namespace MvcControlsToolkit.Core.Options
{
    public class DefaultOptionsDictionary: IOptionsDictionary
    {
        private class OptionEntry
        {
            public uint? Priority;
            public string Value;
            public OptionEntry(string value, uint? priority=null)
            {
                Value = value;
                Priority = priority;
            }
            public void Update(string value, uint? priority)
            {
                if (priority.HasValue)
                {
                    if(!Priority.HasValue || priority.Value >= Priority.Value)
                    {
                        Priority = priority;
                        Value = value;
                    }
                }
                else if (!Priority.HasValue)
                {
                    Priority = priority;
                    Value = value;
                }
            }
        }
        private Dictionary<string, OptionEntry> store = new Dictionary<string, OptionEntry>();
        private Func<Type, object> creator = Activator.CreateInstance;
        public DefaultOptionsDictionary(Func<Type, object> creator = null)
        {
            if (creator != null) this.creator = creator;
        }

        public void AddOption(string name, string value, uint? priority = default(uint?))
        {
            if (name == null) throw new ArgumentException("name");
            OptionEntry curr = null;
            if (store.TryGetValue(name, out curr))
            {
                curr.Update(value, priority);
            }
            else store.Add(name, new OptionEntry(value, priority));
        }

        public void Remove(string name)
        {
            if (name == null) throw new ArgumentException("name");
            store.Remove(name);
        }

        public object GetOptionObject(string prefix, Type type)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (type == null) throw new ArgumentNullException("type");
            var converter = TypeConvertersCache.GetInverseConverter(type);
            if (converter != null) return converter(store[prefix].Value);
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.GetTypeInfo().IsGenericType && (new Type[] { typeof(IEnumerable<>), typeof(ICollection<>) }).Contains(type.GetGenericTypeDefinition()))
                {
                    var innerType = type.GetGenericArguments()[0];
                    converter = TypeConvertersCache.GetInverseConverter(type);
                    if (converter == null) return null;
                    var arrs = store[prefix].Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    var res = Array.CreateInstance(innerType, arrs.Length);
                    int index = 0;
                    foreach(var x in arrs)
                    {
                        res.SetValue(converter(x),index);
                    }
                    return res;
                }
                else return null;
            }
            //complex object
            var obj = creator(type);
            foreach(var prop in type.GetProperties(System.Reflection.BindingFlags.Public))
            {
                prop.SetValue(obj, GetOptionObject(prefix + "." + prop.Name, prop.PropertyType));
            }
            return obj;    
        }

        public void AddOptionObject(string prefix, object obj, uint? priority = default(uint?))
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (obj == null) throw new ArgumentNullException("obj");
            var type = obj.GetType();
            var converter = TypeConvertersCache.GetConverter(type);
            if (converter != null)
            {
                AddOption(prefix, converter(obj), priority);
                return;
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.GetTypeInfo().IsGenericType && (new Type[] { typeof(IEnumerable<>), typeof(ICollection<>) }).Contains(type.GetGenericTypeDefinition()))
                {
                    var innerType = type.GetGenericArguments()[0];
                    converter = TypeConvertersCache.GetConverter(type);
                    if (converter == null) return;
                    StringBuilder sb = new StringBuilder();
                    foreach(var x in obj as IEnumerable)
                    {
                        if (sb.Length > 0) sb.Append("|");
                        sb.Append(converter(x));
                    }
                    AddOption(prefix, sb.ToString(), priority);
                    return;
                }
                else return;
                
            }
            foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public))
            {
                AddOptionObject(prefix + "." + prop.Name, prop.GetValue(obj), priority);
            }
        }
    }
}
