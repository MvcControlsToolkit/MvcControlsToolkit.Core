using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    public enum QueryOptions { None = 0, Equal = 1, NotEqual = 2, LessThan = 4, LessThanOrEqual = 8, GreaterThan = 16, GreaterThanOrEqual = 32, StartsWith = 64, EndsWith = 128, Contains = 256, IsContainedIn = 512, OrderBy=1024, GroupBy=2048}
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class QueryAttribute: Attribute
    {
        public QueryOptions Allow { get; set; }
        public QueryOptions Deny { get; set; }

        public QueryAttribute()
        {
            Allow = QueryOptions.Equal |
                QueryOptions.LessThan |
                QueryOptions.LessThanOrEqual |
                QueryOptions.GreaterThan |
                QueryOptions.GreaterThanOrEqual |
                QueryOptions.NotEqual |
                QueryOptions.StartsWith |
                QueryOptions.EndsWith |
                QueryOptions.IsContainedIn |
                QueryOptions.Contains |
                QueryOptions.OrderBy |
                QueryOptions.GroupBy
                ;
            Deny = QueryOptions.IsContainedIn |
                QueryOptions.Contains;
        }
        public bool Allowed(QueryOptions condition)
        {

            return (condition & Allow & (~Deny)) == condition;

        }
        public QueryOptions Filter(QueryOptions conditions)
        {
            return conditions & Allow & (~Deny);
        }
        public static QueryOptions AllowedForProperty(QueryOptions conditions, Type type, string propertyName)
        {
            if (type == null) throw new ArgumentException(nameof(type));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException(nameof(propertyName));

            PropertyAccessor pa = null;
            try
            {
                pa = new PropertyAccessor(propertyName, type);
            }
            catch { }
            if (pa == null) throw new ArgumentException(nameof(propertyName));
            type = pa.Property.PropertyType;
            QueryAttribute[] attributes = pa[typeof(QueryAttribute)] as QueryAttribute[];
            if (attributes.Length == 0) return QueryOptions.None;
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & QueryOptions.Equal;
                return attributes[0].Filter(conditions);
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return attributes[0].Filter(conditions);
        }
        public QueryOptions AllowedForProperty(QueryOptions conditions, Type propertyType)
        {
            if (propertyType == null) throw new ArgumentException(nameof(propertyType));

            
            var type = propertyType;
            
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & QueryOptions.Equal;
                return this.Filter(conditions);
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return this.Filter(conditions);
        }
        public static QueryOptions AllowedForProperty(Type type, string propertyName)
        {
            if (type == null) throw new ArgumentException(nameof(type));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException(nameof(propertyName));

            PropertyAccessor pa = null;
            try
            {
                pa = new PropertyAccessor(propertyName, type);
            }
            catch { }
            if (pa == null) throw new ArgumentException(nameof(propertyName));
            type = pa.Property.PropertyType;
            QueryAttribute[] attributes = pa[typeof(QueryAttribute)] as QueryAttribute[];
            if (attributes.Length == 0) return QueryOptions.None;

            QueryOptions conditions = attributes[0].Allow & (~attributes[0].Deny);
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & QueryOptions.Equal;
                return conditions;
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return conditions|QueryOptions.OrderBy;
        }

        public QueryOptions AllowedForProperty(Type propertyType)
        {
            if (propertyType == null) throw new ArgumentException(nameof(propertyType));

            var type = propertyType;
            

            QueryOptions conditions = this.Allow & (~this.Deny);
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & QueryOptions.Equal;
                return conditions;
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return conditions;
        }

    }
}
