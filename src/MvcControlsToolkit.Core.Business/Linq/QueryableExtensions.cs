using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MvcControlsToolkit.Core.Linq.Internal;

namespace MvcControlsToolkit.Core.Linq
{
    public static class QueryableExtensions
    {
        private static MethodInfo findMethod(string methodName) {
            return typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2);
        }
        private static MethodInfo orderBy = findMethod("OrderBy");
        private static MethodInfo orderByDescending = findMethod("OrderByDescending");
        private static MethodInfo thenBy = findMethod("ThenBy");
        private static MethodInfo thenByDescending = findMethod("ThenByDescending");

        public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source)
        {
            return new ProjectionExpression<TSource>(source);
        }
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, LambdaExpression property)
        {
            return ApplyOrder<T>(source, property, orderBy);
        }
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, LambdaExpression property)
        {
            return ApplyOrder<T>(source, property, orderByDescending);
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, LambdaExpression property)
        {
            return ApplyOrder<T>(source, property, thenBy);
        }
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, LambdaExpression property)
        {
            return ApplyOrder<T>(source, property, thenByDescending);
        }
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, LambdaExpression property, MethodInfo genericMethod)
        {
            

            object result = genericMethod
                    .MakeGenericMethod(typeof(T), property.ReturnType)
                    .Invoke(null, new object[] { source, property });
            return (IOrderedQueryable<T>)result;
        }
    }
    internal class PropertyBinding
    {
        public PropertyBinding(MemberInfo destination, List<PropertyInfo> properties)
        {
            Destination = destination;
            Sources = properties;
        }
        public MemberInfo Destination { get; set; }
        public bool HasNested {get{ return Sources != null && Sources.Count > 1; } } 
        public void AddPostfix(PropertyInfo property)
        {
            Sources.Add(property);
        }
        public void AddPostfixes(IEnumerable<PropertyInfo> propertyI)
        {
            Sources.AddRange(propertyI);
        }
        public void AddPrefix(PropertyInfo property)
        {
            Sources.Insert(0, property);
        }
        public void AddPrefixes(IEnumerable<PropertyInfo> propertyI)
        {
            Sources.InsertRange(0, propertyI);
        }
        public List<PropertyInfo> Sources { get; set; }
    }
    public class ProjectionExpression<TSource>
    {
        private static readonly ConcurrentDictionary<string, Expression> ExpressionCache = new ConcurrentDictionary<string, Expression>();
        private static readonly ConcurrentDictionary<string, IEnumerable<PropertyBinding>> PropertyBindingCache = new ConcurrentDictionary<string, IEnumerable<PropertyBinding>>();

        private readonly IQueryable<TSource> _source;

        public ProjectionExpression(IQueryable<TSource> source)
        {
            _source = source;
        }

        public IQueryable<TDest> To<TDest>()
        {
 	        var queryExpression = GetCachedExpression<TDest>() ?? BuildExpression<TDest>(null);

            return _source.Select(queryExpression);
        }
        public IQueryable<TDest> To<TDest>(Expression<Func<TSource, TDest>> custom, Type filter=null)
        {
            var queryExpression = BuildExpression<TDest>(custom, filter);

            return _source.Select(queryExpression);
        }

        private static Expression<Func<TSource, TDest>> GetCachedExpression<TDest>(Type source = null)
        {
            var key = GetCacheKey<TDest>(source);

            return ExpressionCache.ContainsKey(key) ? ExpressionCache[key] as Expression<Func<TSource, TDest>> : null;
        }

        private static IEnumerable<PropertyBinding> GetCachedPropertyInfo<TDest>(Type source=null, Type filter = null)
        {
            var key = GetCacheKey<TDest>(source, filter);

            return PropertyBindingCache.ContainsKey(key) ? PropertyBindingCache[key] as IEnumerable<PropertyBinding> : null;
        }
        private static IEnumerable<PropertyBinding> GetCachedPropertyInfo(Type t, Type source = null, Type filter = null, string prefix = null)
        {
            var key = GetCacheKey(t, source, filter, prefix);

            return PropertyBindingCache.ContainsKey(key) ? PropertyBindingCache[key] as IEnumerable<PropertyBinding> : null;
        }

        private static IEnumerable<PropertyBinding> BuildBindings<TDest>(Type source = null, Type filter=null)
        {
            return BuildBindings(typeof(TDest), source, filter);
        }
        private static IEnumerable<PropertyBinding> BuildBindings(Type t, Type source=null, Type filter = null, string prefix=null)
        {
            var res = GetCachedPropertyInfo(t, source, filter, prefix);
            if (res != null) return res;
            var sourceProperties = (source??typeof(TSource)).GetProperties();
            var destinationProperties = t.GetProperties()
                .Where(dest => dest.CanWrite && (dest.PropertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(dest.PropertyType)));
            if (filter != null)
            {
                var filterProperties = new HashSet<string>(filter.GetProperties()
                    .Select(m => m.Name));
                destinationProperties = destinationProperties.Where(m => filterProperties.Contains(m.Name));
            }
            var parameterExpression = Expression.Parameter(typeof(TSource), "src");

            var bindings = destinationProperties
                                .Select(destinationProperty => BuildBindingAssociation(destinationProperty, sourceProperties, prefix))
                                .Where(binding => binding != null);



            var key = GetCacheKey(t, source, filter, prefix);
            try
            {
                PropertyBindingCache.TryAdd(key, bindings);
            }
            catch { }
            return bindings;
        }
        private static MethodCallExpression getNestedSelect(Expression node)
        {
            if (node == null) return null;
            if (node.NodeType == ExpressionType.Call)
            {
                var call = node as MethodCallExpression;
                if (call == null) return null;
                if (call.Method?.Name == "Select") return call;
                if (call.Arguments.Count == 1) return getNestedSelect(call.Arguments[0]);
                return null;
            }
            return null;

        }
        private static Expression copyCallChain(MethodCallExpression start, MethodCallExpression stop, Expression replace)
        {
            if (start == stop) return replace;
            return Expression.Call(start.Method, copyCallChain(start.Arguments[0] as MethodCallExpression, stop, replace));
        }
        private Stack<PropertyInfo>  addToStack(Stack<PropertyInfo> properties, PropertyInfo property)
        {
            if (properties == null) properties = new Stack<PropertyInfo>();
            properties.Push(property);
            return properties;
        }
        private static Expression copyMemberAccesses(Expression x)
        {
            return x;
        }
        private static MemberInitExpression completeMemberInit(
            MemberInitExpression node, 
            ParameterExpression parameterExpression,
            Type filter = null,
            string prefix=null,
            Stack<PropertyInfo> sourceProperties=null,
            ObjectChangesRegister changesRegister=null)
        {
            var customAssignements = node.Bindings.Where(m => m.BindingType == MemberBindingType.Assignment).Select(m => m as MemberAssignment).ToList();
            var assignedProperties = customAssignements.Select(m => m.Member.Name).ToList();
            var internalProjections = customAssignements.Where(m => getNestedSelect(m.Expression) != null);
            var otherAssignements = customAssignements.Except(internalProjections);
            List<MemberAssignment> modifiedAssignements = null;
            List<MemberAssignment> modifiedAssignementsOld = null;
            if (internalProjections != null)
            {
                foreach(var projection in internalProjections)
                {
                    var select = getNestedSelect(projection.Expression);
                    var exp= select.Arguments[1] as LambdaExpression;
                    var originalCollection = select.Arguments[0] as MemberExpression;
                    if (exp != null)
                    {


                        Type childFilter = null;
                        var propertyInfo = (projection.Member as PropertyInfo);
                        TypeInfo childType = propertyInfo.PropertyType.GetTypeInfo();
                        if (childType.IsGenericType && childType.GenericTypeArguments.Length == 1 && childType.GenericTypeArguments[0].GetTypeInfo().IsInterface)
                        {
                            childFilter = childType.GenericTypeArguments[0];
                        }
                        var childChangesRegister = changesRegister == null ? null:  new ObjectChangesRegister(propertyInfo, true, childType.GenericTypeArguments[0], null,originalCollection.Member as PropertyInfo);
                        var newExpression = copyCallChain(projection.Expression as MethodCallExpression, select,
                                Expression.TypeAs(
                                    Expression.Call(select.Method, originalCollection, BuildInternalExpression(exp, childFilter, childChangesRegister)),
                                select.Method.ReturnType)
                                );
                        var newAssignement = changesRegister == null ? Expression.Bind(projection.Member,
                            newExpression) 
                            :
                            Expression.Bind(projection.Member,
                            Expression.Condition(Expression.Equal(copyMemberAccesses(select.Arguments[0]), Expression.Constant(null)),
                                Expression.Constant(null, (newExpression as MethodCallExpression).Method.ReturnType),
                                newExpression));
                        if(changesRegister != null)
                        {
                            childChangesRegister.SetExpression(newExpression);
                            changesRegister.AddChange(childChangesRegister);
                        }
                            
                        if (modifiedAssignements == null)
                        {
                            modifiedAssignements = new List<MemberAssignment>();
                            modifiedAssignementsOld = new List<MemberAssignment>();
                        }
                        modifiedAssignements.Add(newAssignement);
                        modifiedAssignementsOld.Add(projection);
                        
                        
                    }
                }
            }
            if (otherAssignements != null)
            {
                foreach (var assignement in otherAssignements)
                {
                    var newChange = changesRegister == null ? null : new ObjectChangesRegister(assignement.Member as PropertyInfo, false, null, null);
                    var newNode = processTreeRec(assignement.Expression, parameterExpression, filter, prefix, sourceProperties, assignement.Member as PropertyInfo, newChange);
                    if (changesRegister != null)
                    {
                        newChange.SetExpression(newNode);
                        changesRegister.AddChange(newChange);
                    }
                    if (newNode != assignement.Expression)
                    {
                        var newAssignement = Expression.Bind(assignement.Member, newNode);
                        if (modifiedAssignements == null)
                        {
                            modifiedAssignements = new List<MemberAssignment>();
                            modifiedAssignementsOld = new List<MemberAssignment>();
                        }
                        modifiedAssignements.Add(newAssignement);
                        modifiedAssignementsOld.Add(assignement);
                    }
                }
            }
            var innerBindings = BuildBindings(node.NewExpression.Type,
                    sourceProperties != null && sourceProperties.Count > 0 ?
                        sourceProperties.Peek().PropertyType :
                        parameterExpression.Type,
                    filter, prefix)
                    .Where(m => !assignedProperties.Contains(m.Destination.Name));
            IEnumerable<MemberAssignment> bindings;
            if (innerBindings != null && changesRegister != null)
            {
                var lbindings = new List<MemberAssignment>();
                foreach (var binding in innerBindings)
                {
                    var bind = BuildBinding(parameterExpression, binding, sourceProperties);
                    lbindings.Add(bind);
                    changesRegister
                         .AddChange(new ObjectChangesRegister(binding.Destination as PropertyInfo, false, null, bind.Expression));
                }
                bindings= lbindings.Union(customAssignements);
            }
            else
            {
                bindings = innerBindings
                        .Select(m => BuildBinding(parameterExpression, m, sourceProperties))
                        .Union(customAssignements);
            }
            if(modifiedAssignements != null && modifiedAssignements.Count>0)
                bindings= bindings.Except(modifiedAssignementsOld)
                    .Union(modifiedAssignements);
            return Expression.MemberInit(node.NewExpression, bindings);
        }
        private static MemberInitExpression createMemberInit<TDest>(ParameterExpression parameterExpression, Type filter=null, ObjectChangesRegister changesRegister=null)
        {
            var internalBindings = BuildBindings<TDest>(filter);
            IEnumerable<MemberAssignment> bindings;
            if (changesRegister != null )
            {
                changesRegister.MoveToComplex();
                var lbindings = new List<MemberAssignment>();
                if (internalBindings != null)
                {
                    foreach (var binding in internalBindings)
                    {
                        var bind = BuildBinding(parameterExpression, binding);
                        lbindings.Add(bind);
                        changesRegister
                         .AddChange(new ObjectChangesRegister(binding.Destination as PropertyInfo, false, null, bind.Expression));
                    }
                }
                bindings = lbindings;             
            }
            else
            {
                bindings =
                internalBindings
                    .Select(m => BuildBinding(parameterExpression, m));
            }
            
            return Expression.MemberInit(Expression.New(typeof(TDest)), bindings);
        }
        private static MemberInitExpression createMemberInit(ParameterExpression parameterExpression, Type destination, Type source=null, Type filter = null, ObjectChangesRegister changesRegister = null)
        {
            var internalBindings = BuildBindings(destination, source, filter);
            IEnumerable<MemberAssignment> bindings;
            if (changesRegister != null )
            {
                changesRegister.MoveToComplex();
                var lbindings = new List<MemberAssignment>();
                if (internalBindings != null)
                {
                    
                    foreach (var binding in internalBindings)
                    {
                        var bind = BuildBinding(parameterExpression, binding);
                        lbindings.Add(bind);
                        changesRegister
                         .AddChange(new ObjectChangesRegister(binding.Destination as PropertyInfo, false, null, bind.Expression));
                    }
                    
                }
                bindings = lbindings;
            }
            else
            {
                bindings = internalBindings
                    .Select(m => BuildBinding(parameterExpression, m));
            }
            
            return Expression.MemberInit(Expression.New(destination), bindings);
        }
        private static Expression processTreeRec(
            Expression node,
            ParameterExpression parameterExpression,
            Type filter = null,
            string prefix=null,
            Stack<PropertyInfo> sourceProperties = null,
            PropertyInfo currProperty=null, 
            ObjectChangesRegister changesRegister= null)
        {
            if (node.NodeType == ExpressionType.MemberInit)
            {
                if (changesRegister != null) changesRegister.MoveToComplex();
                var currType = sourceProperties != null &&
                    sourceProperties.Count > 0 ? sourceProperties.Peek().PropertyType : parameterExpression.Type;
                string newPrefix = prefix == null ? currProperty?.Name : prefix + currProperty?.Name; ;
                var sourceProperty= newPrefix == null ? null : currType.GetProperty(newPrefix);
                if (currProperty != null && currProperty.PropertyType.GetTypeInfo().IsInterface)
                {
                    filter = currProperty.PropertyType;
                }
                else filter = null;
                if (sourceProperty != null)
                {
                    if (sourceProperties == null) sourceProperties = new Stack<PropertyInfo>();
                    sourceProperties.Push(sourceProperty);
                    var res = completeMemberInit(node as MemberInitExpression, parameterExpression, filter, null, sourceProperties, changesRegister);
                    sourceProperties.Pop();
                    if (sourceProperties.Count == 0) sourceProperties = null;
                    return res;
                }
                else
                {
                     
                    return completeMemberInit(node as MemberInitExpression, parameterExpression, filter, newPrefix, sourceProperties, changesRegister);
                }
                
            }
                
            else if (node.NodeType == ExpressionType.Conditional)
            {
                var cond = node as ConditionalExpression;
                var ifTrue = processTreeRec(cond.IfTrue, parameterExpression, filter, prefix, sourceProperties, currProperty, changesRegister);
                var ifFalse = processTreeRec(cond.IfFalse, parameterExpression, filter, prefix, sourceProperties, currProperty, changesRegister);
                if (ifTrue == cond.IfTrue && ifFalse == cond.IfFalse) return node;
                return Expression.Condition(cond.Test,
                    ifTrue,
                    ifFalse);
            }
            else if (node.NodeType == ExpressionType.Convert)
            {
                var conv = node as UnaryExpression;
                var toConvert = processTreeRec(conv.Operand, parameterExpression, filter, prefix, sourceProperties, currProperty, changesRegister);
                if (toConvert == conv.Operand) return node;
                return Expression.Convert(toConvert,
                    conv.Type, conv.Method);
            }
            else if (node.NodeType == ExpressionType.ConvertChecked)
            {
                var conv = node as UnaryExpression;
                var toConvert = processTreeRec(conv.Operand, parameterExpression, filter, prefix, sourceProperties, currProperty, changesRegister);
                if (toConvert == conv.Operand) return node;
                return Expression.ConvertChecked(toConvert,
                    conv.Type, conv.Method);
            }
            else if (node.NodeType == ExpressionType.New)
            {
                if (changesRegister != null) changesRegister.MoveToComplex();
                return node;
            }
            else return node;
        }
        public static Expression<Func<TSource, TDest>> BuildExpression<TDest>(Expression<Func<TSource, TDest>> custom, Type filter=null, ObjectChangesRegister changesRegister=null)
        {
            ParameterExpression parameterExpression = custom == null ? Expression.Parameter(typeof(TSource), "src") : custom.Parameters.First();
            Expression pres;
            if (custom == null)
            {
                pres = createMemberInit<TDest>(parameterExpression, filter, changesRegister);
            }
            else
            {
                pres = processTreeRec(custom.Body, parameterExpression, filter, null, null, null, changesRegister);
            }

            var expression = Expression.Lambda<Func<TSource, TDest>>(pres, parameterExpression);
            if (custom == null) { 
                var key = GetCacheKey<TDest>();
                try
                {
                    ExpressionCache.TryAdd(key, expression);
                }
                catch { }
            }

            return expression;
        }
        internal static LambdaExpression BuildInternalExpression(LambdaExpression exp, Type filter = null, ObjectChangesRegister changesRegister=null)
        {
            ParameterExpression parameterExpression = exp.Parameters.First();
            if (exp.Body.NodeType == ExpressionType.MemberInit)
            {
                var bindings = (exp.Body as MemberInitExpression).Bindings;
                if (bindings == null || bindings.Count == 0)
                {
                    var res = Expression.Lambda(
                    createMemberInit(parameterExpression, exp.ReturnType, parameterExpression.Type, filter, changesRegister),
                    parameterExpression);
                    var key = GetCacheKey(exp.ReturnType, parameterExpression.Type, filter);
                    try
                    {
                        ExpressionCache.TryAdd(key, res);
                    }
                    catch { }
                    return res;
                }
            }
            var pres = processTreeRec(exp.Body, parameterExpression, filter, null, null, null, changesRegister);
            return Expression.Lambda(pres, parameterExpression);
        }
        private static PropertyBinding BuildBindingAssociation(MemberInfo destinationProperty, IEnumerable<PropertyInfo> sourceProperties, string prefix=null)
        {
            var propertyName = prefix == null ? destinationProperty.Name : prefix+destinationProperty.Name;
            var propertyType = (destinationProperty as PropertyInfo).PropertyType;
            var sourceProperty = sourceProperties.FirstOrDefault(src => src.Name == propertyName );

            if (sourceProperty != null)
            {
                if (propertyType.IsAssignableFrom(Nullable.GetUnderlyingType(sourceProperty.PropertyType)?? sourceProperty.PropertyType))
                    return new PropertyBinding(destinationProperty, new List<PropertyInfo> { sourceProperty });
                else
                    return null;
            }

            PropertyBinding result = null;
            var currProperties = sourceProperties;
            var currName = propertyName;
            while ((sourceProperty = currProperties.FirstOrDefault(src => currName.StartsWith(src.Name) 
                && propertyName.Length > src.Name.Length && Char.IsUpper(propertyName[src.Name.Length]) )) != null)
            {
                if (result == null) result = new PropertyBinding(destinationProperty, new List<PropertyInfo> { sourceProperty });
                else result.AddPostfix(sourceProperty);
                currName = currName.Substring(sourceProperty.Name.Length);
                currProperties = sourceProperty.PropertyType.GetProperties();
                var sourceChildProperty = currProperties.FirstOrDefault(src => src.Name == currName);
                if(sourceChildProperty != null)
                {
                    result.AddPostfix(sourceChildProperty);
                    return result;
                }
            }

            return null;
        }
        private static MemberAssignment BuildBinding(Expression parameterExpression, PropertyBinding binding, Stack<PropertyInfo> prefix=null)
        {
            var dType = (binding.Destination as PropertyInfo).PropertyType;
            
            if (!binding.HasNested && (prefix == null || prefix.Count == 0) )
            {
                var prop = binding.Sources[0];
                var nType = Nullable.GetUnderlyingType(prop.PropertyType);
                var propExp = Expression.Property(parameterExpression, prop);
                return Expression.Bind(binding.Destination, 
                    dType == prop.PropertyType ?
                    propExp as Expression:
                    (dType == nType ?
                     Expression.Condition(Expression.Equal(propExp, Expression.Constant(null, prop.PropertyType)),
                        Expression.Constant(Activator.CreateInstance(nType), dType),
                        Expression.Convert(propExp, dType) as Expression) 
                     :
                     Expression.Convert(propExp, dType) as Expression));
            }
            else
            {
                Expression curr = parameterExpression;
                PropertyInfo lastProp = null;
                List<PropertyInfo> loop = binding.Sources;
                if(prefix != null && prefix.Count > 0)
                {
                    loop = new List<PropertyInfo>(prefix);
                    loop.AddRange(binding.Sources);
                }
                foreach (var property in loop)
                {
                    curr = Expression.Property(curr, property);
                    lastProp = property;
                }
                if (lastProp.PropertyType != dType)
                {
                    var nType = Nullable.GetUnderlyingType(lastProp.PropertyType);
                    if (Nullable.GetUnderlyingType(lastProp.PropertyType) == dType)
                        curr = Expression.Condition(Expression.Equal(curr, Expression.Constant(null, lastProp.PropertyType)),
                        Expression.Constant(Activator.CreateInstance(nType), dType),
                        Expression.Convert(curr, dType) as Expression);
                    else
                        curr = Expression.Convert(curr, dType);
                }
                    
                return Expression.Bind(binding.Destination, curr);
            }
            
        }
   

        private static string GetCacheKey<TDest>(Type source=null, Type filter = null, string prefix=null)
        {
            return GetCacheKey(typeof(TDest), source, filter, prefix);
            
        }
        private static string GetCacheKey(Type t, Type source=null, Type filter = null, string prefix=null)
        {
            return string.Concat((source??typeof(TSource)).FullName, "#" + t.FullName, filter == null ? string.Empty : "#" + filter.FullName, prefix==null ? string.Empty : "#"+prefix);
        }


    }    
}
