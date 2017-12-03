using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.DataAnnotations;

namespace MvcControlsToolkit.Core.Linq.Internal
{
    public class ObjectChangesRegister
    {
        public Type EnumType { get; private set; }
        public bool IsCollection { get; private set; }
        public bool ToAdd { get; private set; }
        public PropertyInfo  KeyProperty { get; private set; }
        public PropertyInfo Property { get; private set;}
        public Expression ValueExpression { get; private set; }
        public List<ObjectChangesRegister> Changes { get; private set; }
        public Dictionary<PropertyInfo,ObjectChangesRegister> Index { get; private set; }
        MethodInfo clear, add, remove;
        Type listType;
        public void SetExpression(Expression value)
        {
            if (ValueExpression == null) ValueExpression = value;
        }
        internal void IndexProperties()
        {
            if (Index != null || Changes == null || Changes.Count == 0) return;
            Index = new Dictionary<PropertyInfo, ObjectChangesRegister>();
            foreach(var o in Changes)
            {
                Index.Add(o.Property, o);
                o.IndexProperties();
            }
        }
        private LambdaExpression FindExpression(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Convert) return FindExpression((exp as UnaryExpression).Operand);
            else if (exp.NodeType == ExpressionType.Conditional)
            {
                var cond = exp as ConditionalExpression;
                var r1 = FindExpression(cond.IfTrue);
                var r2 = FindExpression(cond.IfFalse);
                if (r1 != null && r2 != null) return null;
                else return r1 ?? r2;
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                var currExpression = exp;
                var currProperty = (currExpression as MemberExpression)?.Member as PropertyInfo;
                while (currProperty != null)
                {
                    currExpression = (currExpression as MemberExpression).Expression;
                    currProperty = (currExpression as MemberExpression)?.Member as PropertyInfo;

                }
                if (currExpression.NodeType == ExpressionType.Parameter)
                {
                    var par = currExpression as ParameterExpression;
                    return Expression.Lambda(exp, par);
                }
                else return null;
            }
            else return null;
        }
        public LambdaExpression InvertPropertyChain(List<PropertyInfo> chain, int index =-0)
        {
            if (chain == null || chain.Count < index) return null;
            if (chain.Count > index)
            {
                ObjectChangesRegister next;
                if (Index.TryGetValue(chain[index], out next)) return next.InvertPropertyChain(chain, index + 1);
                else return null;
            }
            else return FindExpression(ValueExpression);
        }
        public ObjectChangesRegister (
            PropertyInfo property,
            bool isComplex=false,
            Type enumType=null,
           Expression value=null,
           PropertyInfo sourceProperty=null)
        {
            Property = property;
            ValueExpression = value;
            if (!isComplex) return;

            Changes = new List<ObjectChangesRegister>();
            EnumType = enumType;
            if(EnumType != null)
            {
                var att= Property.GetCustomAttribute(typeof(CollectionKeyAttribute)) as CollectionKeyAttribute;
                KeyProperty = att != null ? EnumType.GetProperty(att.KeyName) : null;
                var sAtt = sourceProperty == null ? null :
                    sourceProperty.GetCustomAttribute(typeof(CollectionChangeAttribute)) as CollectionChangeAttribute;
                ToAdd = sAtt != null && sAtt.Mode == CollectionChangeMode.Add;
                var collectionType = typeof(ICollection<>).GetTypeInfo().MakeGenericType(new Type[] { enumType });
                IsCollection = collectionType.IsAssignableFrom(Property.PropertyType);
                
                add = collectionType.GetMethod("Add");
                clear = collectionType.GetMethod("Clear");
                remove = collectionType.GetMethod("Remove");
                listType = typeof(List<>).GetTypeInfo().MakeGenericType(new Type[] { enumType });

            }
        }
        public void AddChange(ObjectChangesRegister change)
        {
            if (Changes == null) return;
            Changes.Add(change);
        }
        public void MoveToComplex()
        {
            if(Changes == null)
                Changes = new List<ObjectChangesRegister>();
        }
        public List<string> ComputePaths(out bool hasIenumerables)
        {
            hasIenumerables = this.EnumType != null;
            if (Changes == null) return null;
            List<string> res = new List<string>();
            foreach(var change in Changes)
            {
                hasIenumerables = hasIenumerables || change.EnumType != null;
                if (change.Changes != null)
                {
                    bool innerHasIenumerables;
                    var innerRes = change.ComputePaths(out innerHasIenumerables);
                    if (innerRes == null || innerRes.Count==0) res.Add(change.Property.Name);
                    else
                    {
                        foreach (var s in innerRes)
                            res.Add(change.Property.Name + "." + s);
                    }
                    hasIenumerables = hasIenumerables || innerHasIenumerables;
                }
                    
            }
            return res;
        }
        public void CopyChanges(object source, object destination)
        {
            
            foreach(var change in Changes)
            {
                if(change.Changes == null)
                {
                    change.Property.SetValue(destination, change.Property.GetValue(source));
                }
                else if(change.EnumType == null)
                {
                    object newSource = change.Property.GetValue(source);
                    object newDestination = change.Property.GetValue(destination);
                    if (newDestination == null || newSource == null)
                        change.Property.SetValue(destination, newSource);
                    else 
                        change.CopyChanges(newSource, newDestination);
                }
                else
                {
                    object sourceEnum = change.Property.GetValue(source);
                    object destinationEnum = change.Property.GetValue(destination);
                    if(destinationEnum == null || (!change.IsCollection && change.KeyProperty == null && !change.ToAdd ))
                        change.Property.SetValue(destination, sourceEnum);
                    else 
                    {
                        bool alreadyCleared = false;
                        IEnumerable oldDestination = null;
                        if (!change.IsCollection)
                        {
                            var newDestinationEnum = Activator.CreateInstance(change.listType);
                            if (change.ToAdd )
                            {
                                foreach(var item in destinationEnum as IEnumerable)
                                    change.add.Invoke(newDestinationEnum, new object[] { item });
                            }
                            else alreadyCleared = true;
                            oldDestination = destinationEnum as IEnumerable;
                            destinationEnum = newDestinationEnum;
                            change.Property.SetValue(destination, destinationEnum);
                        }
                        if(sourceEnum == null)
                        {
                            if (!change.ToAdd && !alreadyCleared)
                                change.clear.Invoke(destinationEnum, new object[0]);
                        }
                        else if (change.KeyProperty==null)
                        {
                            if (!change.ToAdd && !alreadyCleared)
                                change.clear.Invoke(destinationEnum, new object[0]);
                            
                            foreach(var item in sourceEnum as IEnumerable)
                            {
                                change.add.Invoke(destinationEnum, new object[] { item });
                            }
                            
                        }
                        else
                        {
                            if (oldDestination == null)
                            {
                                var list = new List<object>();
                                foreach (var item in destinationEnum as IEnumerable)
                                    list.Add(item);
                                oldDestination = list;
                            }
                            if (!alreadyCleared)
                                change.clear.Invoke(destinationEnum, new object[0]);
                            var dict = new Dictionary<object, object>();
                            if (change.ToAdd)
                            {
                                foreach (var item in sourceEnum as IEnumerable)
                                {
                                    dict.Add(change.KeyProperty.GetValue(item), item);
                                }
                                foreach (var item in oldDestination )
                                {
                                    object newVersion;
                                    var key = change.KeyProperty.GetValue(item);
                                    if (dict.TryGetValue(key, out newVersion))
                                    {
                                        dict.Remove(key);
                                        change.CopyChanges(newVersion, item);
                                        change.add.Invoke(destinationEnum, new object[] { item });
                                    }
                                    else change.add.Invoke(destinationEnum, new object[] { item });
                                }
                                foreach(var item in dict)
                                {
                                    change.add.Invoke(destinationEnum, new object[] { item.Value });
                                }
                            }
                            else
                            {
                                foreach (var item in oldDestination)
                                {
                                    dict.Add(change.KeyProperty.GetValue(item), item);
                                }
                                foreach (var item in sourceEnum as IEnumerable)
                                {
                                    object old;
                                    if (dict.TryGetValue(change.KeyProperty.GetValue(item), out old))
                                    {
                                        change.CopyChanges(item, old);
                                        change.add.Invoke(destinationEnum, new object[] { old });
                                    }
                                    else change.add.Invoke(destinationEnum, new object[] { item });
                                }
                            }
                            
                            
                        }
                    }
                    

                }
            }
        }
    }
}
