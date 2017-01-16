using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

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
        public PropertyBinding (MemberInfo destination, PropertyInfo source, PropertyInfo sourceNested)
        {
            Destination = destination;
            Source = source;
            SourceNested = sourceNested;
        }
        public MemberInfo Destination { get; set; }
        public PropertyInfo Source { get; set; }
        public PropertyInfo SourceNested { get; set; }
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
        public IQueryable<TDest> To<TDest>(Expression<Func<TSource, TDest>> custom)
        {
            var queryExpression = BuildExpression<TDest>(custom);

            return _source.Select(queryExpression);
        }

        private static Expression<Func<TSource, TDest>> GetCachedExpression<TDest>()
        {
            var key = GetCacheKey<TDest>();

            return ExpressionCache.ContainsKey(key) ? ExpressionCache[key] as Expression<Func<TSource, TDest>> : null;
        }

        private static IEnumerable<PropertyBinding> GetCachedPropertyInfo<TDest>()
        {
            var key = GetCacheKey<TDest>();

            return PropertyBindingCache.ContainsKey(key) ? PropertyBindingCache[key] as IEnumerable<PropertyBinding> : null;
        }
        private static IEnumerable<PropertyBinding> GetCachedPropertyInfo(Type t)
        {
            var key = GetCacheKey(t);

            return PropertyBindingCache.ContainsKey(key) ? PropertyBindingCache[key] as IEnumerable<PropertyBinding> : null;
        }

        private static IEnumerable<PropertyBinding> BuildBindings<TDest>()
        {
            return BuildBindings(typeof(TDest));
        }
        private static IEnumerable<PropertyBinding> BuildBindings(Type t)
        {
            var res = GetCachedPropertyInfo(t);
            if (res != null) return res;
            var sourceProperties = typeof(TSource).GetProperties();
            var destinationProperties = t.GetProperties().Where(dest => dest.CanWrite);
            var parameterExpression = Expression.Parameter(typeof(TSource), "src");

            var bindings = destinationProperties
                                .Select(destinationProperty => BuildBindingAssociation(destinationProperty, sourceProperties))
                                .Where(binding => binding != null);



            var key = GetCacheKey(t);
            try
            {
                PropertyBindingCache.TryAdd(key, bindings);
            }
            catch { }
            return bindings;
        }
        private static MemberInitExpression completeMemberInit<TDest>(
            MemberInitExpression node, 
            ParameterExpression parameterExpression)
        {
            var customAssignements = node.Bindings.Where(m => m.BindingType == MemberBindingType.Assignment).Select(m => m as MemberAssignment).ToList();
            var assignedProperties = customAssignements.Select(m => m.Member).ToList();
            var bindings = BuildBindings(node.NewExpression.Type)
                    .Where(m => !assignedProperties.Contains(m.Destination))
                   .Select(m => BuildBinding(parameterExpression, m)).Union(customAssignements);
            return Expression.MemberInit(node.NewExpression, bindings);
        }
        private static MemberInitExpression createMemberInit<TDest>(ParameterExpression parameterExpression)
        {
            var bindings = BuildBindings<TDest>()
                    .Select(m => BuildBinding(parameterExpression, m));
            return Expression.MemberInit(Expression.New(typeof(TDest)), bindings);
        }
        private static Expression processTreeRec<TDest>(
            Expression node,
            ParameterExpression parameterExpression)
        {
            if (node.NodeType == ExpressionType.MemberInit)
                return completeMemberInit<TDest>(node as MemberInitExpression, parameterExpression);
            else if (node.NodeType == ExpressionType.Conditional)
            {
                var cond = node as ConditionalExpression;
                return Expression.Condition(cond.Test,
                    processTreeRec<TDest>(cond.IfTrue, parameterExpression),
                    processTreeRec<TDest>(cond.IfFalse, parameterExpression));
            }
            else if (node.NodeType == ExpressionType.Convert)
            {
                var conv = node as UnaryExpression;
                return Expression.Convert(processTreeRec<TDest>(conv.Operand, parameterExpression),
                    conv.Type, conv.Method);
            }
            else if (node.NodeType == ExpressionType.ConvertChecked)
            {
                var conv = node as UnaryExpression;
                return Expression.ConvertChecked(processTreeRec<TDest>(conv.Operand, parameterExpression),
                    conv.Type, conv.Method);
            }
            else return node;
        }
        public static Expression<Func<TSource, TDest>> BuildExpression<TDest>(Expression<Func<TSource, TDest>> custom)
        {
            ParameterExpression parameterExpression = custom == null ? Expression.Parameter(typeof(TSource), "src") : custom.Parameters.First();
            Expression pres;
            if (custom == null)
            {
                pres = createMemberInit<TDest>(parameterExpression);
            }
            else
            {
                pres = processTreeRec<TDest>(custom.Body, parameterExpression);
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
       
        private static PropertyBinding BuildBindingAssociation(MemberInfo destinationProperty, IEnumerable<PropertyInfo> sourceProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(src => src.Name == destinationProperty.Name);

            if (sourceProperty != null)
            {
                return new PropertyBinding(destinationProperty, sourceProperty, null);
            }

            var propertyName = destinationProperty.Name;

            sourceProperty = sourceProperties.FirstOrDefault(src => propertyName.StartsWith(src.Name) 
                && Char.IsUpper(propertyName[src.Name.Length]) );

            if (sourceProperty != null)
            {
                var propertyNameRemainder = propertyName.Substring(sourceProperty.Name.Length);
                var sourceChildProperty = sourceProperty.PropertyType.GetProperties().FirstOrDefault(src => src.Name == propertyNameRemainder);

                if (sourceChildProperty != null)
                {
                    return new PropertyBinding(destinationProperty, sourceProperty, sourceChildProperty);
                        
                }
            } 
            return null;
        }
        private static MemberAssignment BuildBinding(Expression parameterExpression, PropertyBinding binding)
        {
            var dType = (binding.Destination as PropertyInfo).PropertyType;
            if (binding.SourceNested == null)
            {
                
                return Expression.Bind(binding.Destination, 
                    dType == binding.Source.PropertyType ? 
                    Expression.Property(parameterExpression, binding.Source) as Expression:
                    Expression.Convert(Expression.Property(parameterExpression, binding.Source), dType) as Expression);
            }
            else
            {
                return Expression.Bind(binding.Destination,
                    dType == binding.SourceNested.PropertyType ?
                    Expression.Property(Expression.Property(parameterExpression, binding.Source), binding.SourceNested) as Expression :
                    Expression.Convert(Expression.Property(Expression.Property(parameterExpression, binding.Source), binding.SourceNested), dType) as Expression
                    );
            }
            
        }
   

        private static string GetCacheKey<TDest>()
        {
            return string.Concat(typeof(TSource).FullName, typeof(TDest).FullName);
        }
        private static string GetCacheKey(Type t)
        {
            return string.Concat(typeof(TSource).FullName, t.FullName);
        }


    }    
}
