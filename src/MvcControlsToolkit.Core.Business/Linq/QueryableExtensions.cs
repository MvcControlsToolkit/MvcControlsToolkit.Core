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
        public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source)
        {
            return new ProjectionExpression<TSource>(source);
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
            var queryExpression = custom == null || custom.Body.NodeType == ExpressionType.MemberInit ?
                BuildExpression<TDest>(custom) :
                custom;

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

        private static IEnumerable<PropertyBinding> BuildBindings<TDest>()
        {
            var res = GetCachedPropertyInfo<TDest>();
            if (res != null) return res;
            var sourceProperties = typeof(TSource).GetProperties();
            var destinationProperties = typeof(TDest).GetProperties().Where(dest => dest.CanWrite);
            var parameterExpression = Expression.Parameter(typeof(TSource), "src");

            var bindings = destinationProperties
                                .Select(destinationProperty => BuildBindingAssociation(destinationProperty, sourceProperties))
                                .Where(binding => binding != null);

            

            var key = GetCacheKey<TDest>();
            try
            {
                PropertyBindingCache.TryAdd(key, bindings);
            }
            catch { }
            return bindings;
        }

        private static Expression<Func<TSource, TDest>> BuildExpression<TDest>(Expression<Func<TSource, TDest>> custom)
        {
            ParameterExpression parameterExpression = custom == null ? Expression.Parameter(typeof(TSource), "src") : custom.Parameters.First();
            List<MemberAssignment> customAssignements = null;
            List<MemberInfo> assignedProperties = null;
            if (custom != null && custom.Body.NodeType == ExpressionType.MemberInit)
            {
                customAssignements = (custom.Body as MemberInitExpression).Bindings.Where(m => m.BindingType == MemberBindingType.Assignment).Select(m => m as MemberAssignment).ToList();
                assignedProperties = customAssignements.Select(m => m.Member).ToList();
            }
            IEnumerable<MemberAssignment> bindings = null;
            if (assignedProperties == null || assignedProperties.Count == 0)
            {
                bindings = BuildBindings<TDest>()
                    .Select(m => BuildBinding(parameterExpression, m));
            }
            else
            {
                bindings = BuildBindings<TDest>()
                    .Where(m => !assignedProperties.Contains(m.Destination))
                   .Select(m => BuildBinding(parameterExpression, m)).Union(customAssignements);

            }
            //var bindings1 = BuildBindings<TDest>()
            //    .Select(m => BuildBinding(parameterExpression, m));

            var expression = Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), bindings), parameterExpression);
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

        
    }    
}
