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
            public IOptionsProvider Provider;
            public OptionEntry(IOptionsProvider provider, string value, uint? priority=null)
            {
                Value = value;
                Priority = priority;
                Provider = provider;
            }
            public IOptionsProvider Update(IOptionsProvider provider, string value, uint? priority)
            {
                IOptionsProvider res = null;
                if (priority.HasValue)
                {
                    if(!Priority.HasValue || priority.Value >= Priority.Value)
                    {
                        Priority = priority;
                        if (Value != value && Provider != provider) res = Provider;
                        Value = value;
                        Provider = provider;
                    }
                }
                else if (!Priority.HasValue)
                {
                    Priority = priority;
                    if (Value != value && Provider != provider) res = Provider;
                    Value = value;
                    Provider = provider;
                }
                return res;
            }
        }
        private Dictionary<string, OptionEntry> store = new Dictionary<string, OptionEntry>();
        public Func<Type, object> Creator
        {
            get; set;
        }
        public DefaultOptionsDictionary()
        {
            Creator = Activator.CreateInstance;
        }
        private string propertyName(PropertyInfo property)
        {
            var att = property.GetCustomAttribute(typeof(Attributes.OptionNameAttribute)) as Attributes.OptionNameAttribute;
            if (att == null) return property.Name;
            else return att.Name;
        }
        public IOptionsProvider AddOption(IOptionsProvider provider, string name, string value, uint? priority = default(uint?))
        {
            if (name == null) throw new ArgumentException("name");
            
            IOptionsProvider res = null;
            OptionEntry curr = null;
            if (store.TryGetValue(name, out curr))
            {
                curr.Update(provider, value, priority);
            }
            else store.Add(name, new OptionEntry(provider, value, priority));
            return res;
        }

        public void Remove(string name)
        {
            if (name == null) throw new ArgumentException("name");
            store.Remove(name);
        }
        public List<KeyValuePair<string, string>> GetEntries(string prefix, string newPrefix = null)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (string.IsNullOrWhiteSpace(newPrefix)) newPrefix = null;
            List<KeyValuePair<string, string>> res = new List<KeyValuePair<string, string>>();
            foreach(var x in store)
            {
                if (x.Key == prefix || (x.Key.StartsWith(prefix) && x.Key[prefix.Length] == '.'))
                {
                    res.Add(new KeyValuePair<string, string>(newPrefix== null ? x.Key : newPrefix+x.Key.Substring(prefix.Length), x.Value.Value));

                }
            }
            return res;
        }
        public object GetOptionObject(string prefix, Type type, object instance=null)
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
            var obj = instance?? Creator(type);
            if(obj == null)
            {
                obj = Activator.CreateInstance(type);
                foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public))
                {
                    prop.SetValue(obj, GetOptionObject(prefix + "." + propertyName(prop), prop.PropertyType, instance == null ? null : prop.GetValue(instance)));
                }
            }
            
            return obj;    
        }

        public List<IOptionsProvider> AddOptionObject(IOptionsProvider provider, string prefix, object obj, uint? priority = default(uint?), uint jumpComplexTypes = uint.MaxValue)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (obj == null) throw new ArgumentNullException("obj");
            
            List<IOptionsProvider> res = new List<IOptionsProvider>();
            var type = obj.GetType();
            var converter = TypeConvertersCache.GetConverter(type);
            if (converter != null)
            {
                var pres=AddOption(provider, prefix, converter(obj), priority);
                if (pres != null) res.Add(pres);
                return res;
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.GetTypeInfo().IsGenericType && (new Type[] { typeof(IEnumerable<>), typeof(ICollection<>) }).Contains(type.GetGenericTypeDefinition()))
                {
                    var innerType = type.GetGenericArguments()[0];
                    converter = TypeConvertersCache.GetConverter(type);
                    if (converter == null) return res;
                    StringBuilder sb = new StringBuilder();
                    foreach(var x in obj as IEnumerable)
                    {
                        if (sb.Length > 0) sb.Append("|");
                        sb.Append(converter(x));
                    }
                    var pres = AddOption(provider, prefix, sb.ToString(), priority);
                    if (pres != null) res.Add(pres);
                    return res;
                }
                else return res;
                
            }
            if (jumpComplexTypes > 0)
            {
                foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public))
                {
                    res.AddRange(AddOptionObject(provider, prefix + "." + propertyName(prop), prop.GetValue(obj), priority, jumpComplexTypes - 1));
                }
            }
            return res;
        }
    }
}
