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

namespace MvcControlsToolkit.Core.ModelBinding.DerivedClasses
{
    public class DerivedClassesRegister
    {
        protected static IDictionary<Type, int> AllRunTimeTypes;
        protected static KeyValuePair<Type, int>[] InverseAllRunTimeTypes;
        internal static void Prepare(IHostingEnvironment env)
        {
            int i = 0;
            InverseAllRunTimeTypes = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(env.ApplicationName).
                Where(m=>!m.Name.StartsWith("MvcControlsToolkit"))
                .SelectMany(m => (m as AssemblyPart).Types).Where(m => m.IsPublic 
                && m.GetCustomAttribute(typeof(MvcControlsToolkit.Core.DataAnnotations.RunTimeTypeAttribute))!= null)
                .Select(m => m.AsType())
                .Select(m => new KeyValuePair<Type, int>(m, i++))
                .ToArray();
            
            AllRunTimeTypes=InverseAllRunTimeTypes
                .ToDictionary(m => m.Key, m=> m.Value);
        }
        internal static Type GetTypeFromCode(string index)
        {
            index = index.Substring(0, index.Length - 1);
            int i;
            if (!int.TryParse(index, NumberStyles.Integer, CultureInfo.InvariantCulture, out i)) return null;
            if (i >= InverseAllRunTimeTypes.Length || i<0) return null;
            return InverseAllRunTimeTypes[i].Key;
        }
        public static string GetCodeFromType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            int res;
            if (AllRunTimeTypes.TryGetValue(type, out res)) return res.ToString(CultureInfo.InvariantCulture)+"_";
            else throw new ArgumentException(string.Format(DefaultMessages.SubclassNotRegistered, type.Name), nameof(type)); 
        }
    }
}
