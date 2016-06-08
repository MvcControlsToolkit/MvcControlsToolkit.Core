using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;

namespace MvcControlsToolkit.Core.Views
{
    internal class Instantiation
    {
        public Type Var;
        public Type Value;
    }
    internal class TransformationItem
    {
        internal TransformationItem(int index, Type transformType)
        {
            this.index = index;
            var implementation = transformType.GetInterfaces().Where(m => m.GetTypeInfo().IsGenericType && m.GetGenericTypeDefinition() == typeof(IBindingTransformation<,,>))
                .FirstOrDefault();
            if (implementation != null)
            {
                var args = implementation.GetGenericArguments();
                this.sType = args[0];
                this.iType = args[1];
                this.dType = args[2];
                this.cType = transformType;
            }
        }
        private int index;
        private Type sType, iType, dType, cType;
        public int Index { get { return index; } }
        public Type SType { get { return sType; } }
        public Type IType { get { return iType; } }
        public Type DType { get { return dType; } }
        public Type CType { get { return cType; } }
    }
    
    public class TransformationsRegister
    {
        private static Dictionary<Type, TransformationItem> directDictionary = new Dictionary<Type, TransformationItem>();
        private static Dictionary<int, TransformationItem> inverseDictionary = new Dictionary<int, TransformationItem>();
        private static int count = 0;

        public static void Add(Type m)
        {
            var item = new TransformationItem(++count, m);
            if (item.SType != null)
            {
                inverseDictionary.Add(item.Index, item);
                directDictionary[m] = item;
            }
        }
        public static string GetPrefix<T>()
        {
            var type = typeof(T).GetTypeInfo().IsGenericType ? typeof(T).GetGenericTypeDefinition() : typeof(T);
            TransformationItem res;
            if (directDictionary.TryGetValue(type, out res))
            {
                return res.Index.ToString(CultureInfo.InvariantCulture);
            }
            else return null;
        }
        public static string GetPrefix(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            type = type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : type;
            TransformationItem res;
            if (directDictionary.TryGetValue(type, out res))
            {
                return res.Index.ToString(CultureInfo.InvariantCulture);
            }
            else return null;
        }
        private static object defaultValue(Type x)
        {
            object res = null;
            if (x.GetTypeInfo().IsValueType) res = Activator.CreateInstance(x);
            return res;
        }
        private static void  matchTypes(Type[] abstractType, Type[] concreteType,  Dictionary<Type, Instantiation> instantiations)
        {
            if (abstractType.Length != concreteType.Length) return;
            for(int i =0; i<abstractType.Length; i++)
            {
                var atype = abstractType[i];
                var ctype = concreteType[i];
                var iatype = atype.GetTypeInfo();
                var ictype = ctype.GetTypeInfo();
                Instantiation res;
                if(atype.IsGenericParameter && instantiations.TryGetValue(atype, out res)){
                    res.Value = ctype;
                }
                else if (iatype.IsGenericType && iatype.ContainsGenericParameters)
                {
                    if (!ictype.IsGenericType) return;
                    matchTypes(atype.GetGenericArguments(), ctype.GetGenericArguments(), instantiations);
                }
                else return;
            }
        }
        public static Type InverseTransform(Type destinationType, string index, out Type fitype, out Type fdtype)
        {
            if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
            if (string.IsNullOrWhiteSpace(index)) throw new ArgumentNullException(nameof(index));
            fitype = null;
            fdtype = null;
            int indexer = int.Parse(index);
            TransformationItem res;
            if (!inverseDictionary.TryGetValue(indexer, out res) ) return null;

            Type fctype = null;
            if (res.CType.GetTypeInfo().IsGenericType)
            {
                var instantiations = res.CType.GetGenericArguments().Select(m => new Instantiation
                {
                    Var = m,
                    Value = null
                }).ToArray();
                var dict = new Dictionary<Type, Instantiation>();
                foreach(var x in instantiations)
                {
                    dict.Add(x.Var, x);
                }
                matchTypes(new Type[] { res.DType }, new Type[] { destinationType }, dict);
                foreach(var x in instantiations)
                {
                    if(x.Value==null) return null;
                }
                fctype = res.CType.MakeGenericType(instantiations.Select(m => m.Value).ToArray());
                var implementation = fctype.GetInterfaces().Where(m => m.GetTypeInfo().IsGenericType && m.GetGenericTypeDefinition() == typeof(IBindingTransformation<,,>))
                .FirstOrDefault();
                if (implementation == null) return null;
                var args = implementation.GetGenericArguments();
                fitype = args[1];
                fdtype = args[2];
            }
            else
            {
                fctype = res.CType;
                fdtype = res.DType;
                fitype = res.IType;
            }

            if (!destinationType.IsAssignableFrom(fdtype)) return null;
            return fctype;
            //return fctype.GetMethod("InverseTransform").Invoke(Activator.CreateInstance(fctype), new[] { value });
        }
    }
}
