using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace MvcControlsToolkit.Core.ModelBinding.DerivedClasses
{
    public class DerivedClasssSerializationBinder : DefaultSerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return DerivedClassesRegister.GetTypeFromCode(typeName);
        }
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = DerivedClassesRegister.GetCodeFromType(serializedType, true);
        }
    }
    public class DerivedClassesRegister
    {
        protected static IDictionary<Type, int> AllRunTimeTypes;
        protected static List<KeyValuePair<Type, int>> InverseAllRunTimeTypes;
        protected static List<Type> DefaultTypes;
        protected static object locker = new object();
        internal static void Prepare(IHostingEnvironment env)
        {
            
            var tenum = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(env.ApplicationName).
                Where(m => !m.Name.StartsWith("MvcControlsToolkit"))
                .SelectMany(m => (m as AssemblyPart).Types).Where(m => m.IsPublic
                && m.GetCustomAttribute(typeof(MvcControlsToolkit.Core.DataAnnotations.RunTimeTypeAttribute)) != null)
                .Select(m => m.AsType());
            if (DefaultTypes != null) tenum = tenum.Union(DefaultTypes);
            var tlist = new List<Type>();
            foreach(var t in tenum)
            {
                
                var st = t;
                while(st != null && st != typeof(object))
                {
                    tlist.Add(st);
                    st = st.GetTypeInfo().BaseType;
                }
            }
            int i = 0;
            InverseAllRunTimeTypes = tlist.Distinct().Select(m => new KeyValuePair<Type, int>(m, i++))
            .ToList();

            AllRunTimeTypes =InverseAllRunTimeTypes
                .ToDictionary(m => m.Key, m=> m.Value);
        }
        internal static Type GetTypeFromCode(string index)
        {
            if (index == null || index.Length < 2) return null;
            index = index.Substring(0, index.Length - 1);
            int i;
            if (!int.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out i)) return null;
            if (i >= InverseAllRunTimeTypes.Count || i<0) return null;
            return InverseAllRunTimeTypes[i].Key;
        }
        public static string GetCodeFromType(Type type, bool quiet=false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            int res;
            if (AllRunTimeTypes.TryGetValue(type, out res)) return res.ToString(CultureInfo.InvariantCulture) + "_";
            else if (quiet) return null;
            else throw new ArgumentException(string.Format(DefaultMessages.SubclassNotRegistered, type.Name), nameof(type)); 
        }
        public static void AddDefaultType<T>()
        {
            if (DefaultTypes == null) DefaultTypes = new List<Type>();
            DefaultTypes.Add(typeof(T));
        }
        
        public int GetAndAddIfNotExists(Type t)
        {
            int res;
            if (AllRunTimeTypes.TryGetValue(t, out res)) return res;
            lock (locker)
            {
                if (AllRunTimeTypes.TryGetValue(t, out res)) return res;
                var toAdd = new KeyValuePair<Type, int>(t, InverseAllRunTimeTypes.Count);
                InverseAllRunTimeTypes.Add(toAdd);
                AllRunTimeTypes.Add(toAdd);
                return toAdd.Value;
            }
        }
    }
}
