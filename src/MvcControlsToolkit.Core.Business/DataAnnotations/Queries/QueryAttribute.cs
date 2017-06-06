using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MvcControlsToolkit.Core.DataAnnotations.Queries;

namespace MvcControlsToolkit.Core.DataAnnotations
{

    [Flags]
    public enum QueryOptions: uint { None = 0, Equal = 1, NotEqual = 2, LessThan = 4, LessThanOrEqual = 8, GreaterThan = 16, GreaterThanOrEqual = 32, StartsWith = 64, EndsWith = 128, Contains = 256, IsContainedIn = 512, Search=1024, OrderBy=2048, GroupBy=4096, AllFilters = 1023, All = 8191}
    [Flags]
    public enum GroupingOptions : uint { None = 0, Sum = 1, Average = 2, Min = 4, Max = 8, CountDistinct = 16, Group = 32,  Aggregations = 31, All = 63}

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QuerySearchAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class QueryAttribute : Attribute
    {

        private static KeyValuePair<string, string>[] decoder = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("eq", "="),
            new KeyValuePair<string, string>("ne", "<>"),
            new KeyValuePair<string, string>("lt", "<"),
            new KeyValuePair<string, string>("le", "<="),
            new KeyValuePair<string, string>("gt", ">"),
            new KeyValuePair<string, string>("ge", ">="),
            new KeyValuePair<string, string>("startswith", "starts with"),
            new KeyValuePair<string, string>("endswith", "ends with"),
            new KeyValuePair<string, string>("contains", "contains"),
            new KeyValuePair<string, string>("inv-contains", "contained in")

        };
        private static KeyValuePair<string, string>[] groupDecoder = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("", "none"),
            new KeyValuePair<string, string>("sum", "sum"),
            new KeyValuePair<string, string>("average", "average"),
            new KeyValuePair<string, string>("min", "min"),
            new KeyValuePair<string, string>("max", "max"),
            new KeyValuePair<string, string>("countdistinct", "count distinct"),
            new KeyValuePair<string, string>("groupby", "group by")
        };
        public QueryOptions Allow { get; set; }
        public QueryOptions Deny { get; set; }
        public QueryOptions Add {set { Allow = Allow | value; } get { return Allow; } }
        public QueryOptions Remove { set { Deny = Deny | value; } get { return Deny; } }
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
                QueryOptions.Contains | QueryOptions.Search;
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

            var pInfo = QueryNodeCache.GetPath(type, propertyName);
            type = pInfo.Item1[pInfo.Item1.Count - 1].PropertyType;

            if (pInfo.Item2 == null) return QueryOptions.None;
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & (QueryOptions.Equal | QueryOptions.OrderBy |
                QueryOptions.GroupBy);
                return pInfo.Item2.Filter(conditions);
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            else
            {
                conditions = conditions & (~(QueryOptions.LessThan |
                                             QueryOptions.LessThanOrEqual | 
                                             QueryOptions.GreaterThan | 
                                             QueryOptions.GreaterThanOrEqual));
            }

            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return pInfo.Item2.Filter(conditions);
        }
        public QueryOptions AllowedForProperty(QueryOptions conditions, Type propertyType)
        {
            if (propertyType == null) throw new ArgumentException(nameof(propertyType));


            var type = propertyType;

            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & (QueryOptions.Equal | QueryOptions.OrderBy |
                QueryOptions.GroupBy);
                return this.Filter(conditions);
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            else
            {
                conditions = conditions & (~(QueryOptions.LessThan |
                                             QueryOptions.LessThanOrEqual |
                                             QueryOptions.GreaterThan |
                                             QueryOptions.GreaterThanOrEqual));
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

            var pInfo = QueryNodeCache.GetPath(type, propertyName);
            type = pInfo.Item1[pInfo.Item1.Count - 1].PropertyType;

            if (pInfo.Item2 == null) return QueryOptions.None;

            QueryOptions conditions = pInfo.Item2.Allow & (~pInfo.Item2.Deny);
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & (QueryOptions.Equal | QueryOptions.OrderBy |
                QueryOptions.GroupBy);
                return conditions;
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            else
            {
                conditions = conditions & (~(QueryOptions.LessThan |
                                             QueryOptions.LessThanOrEqual |
                                             QueryOptions.GreaterThan |
                                             QueryOptions.GreaterThanOrEqual));
            }
            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return conditions | QueryOptions.OrderBy;
        }

        public QueryOptions AllowedForProperty(Type propertyType)
        {
            if (propertyType == null) throw new ArgumentException(nameof(propertyType));

            var type = propertyType;


            QueryOptions conditions = this.Allow & (~this.Deny);
            if (type == typeof(bool) || type == typeof(bool?))
            {
                conditions = conditions & (QueryOptions.Equal | QueryOptions.OrderBy |
                QueryOptions.GroupBy);
                return conditions;
            }
            if (type != typeof(string))
            {
                conditions = conditions & (~(QueryOptions.StartsWith |
                                             QueryOptions.EndsWith));
            }
            else
            {
                conditions = conditions & (~(QueryOptions.LessThan |
                                             QueryOptions.LessThanOrEqual |
                                             QueryOptions.GreaterThan |
                                             QueryOptions.GreaterThanOrEqual));
            }
            if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type))
            {
                conditions = conditions & (~QueryOptions.Contains);
            }
            return conditions;
        }
        public static GroupingOptions AllowedAggregationsForType(Type t, QueryOptions query)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;
            var res = GroupingOptions.CountDistinct;
            if ((query & QueryOptions.GroupBy) == QueryOptions.GroupBy)
                res = res | GroupingOptions.Group;
            if (t == typeof(double) || t == typeof(decimal) || t == typeof(long))
            {
                res = res | GroupingOptions.Max
                    | GroupingOptions.Min
                    | GroupingOptions.Sum
                    | GroupingOptions.Average;

            }
            else if (t == typeof(int) || t == typeof(float))
            {
                res = res | GroupingOptions.Max
                    | GroupingOptions.Min
                    | GroupingOptions.Sum;
            }
            return res;
        }
        public static IEnumerable<KeyValuePair<string, string>> QueryOptionsToEnum(QueryOptions options)
        {
            QueryOptions run = (QueryOptions)1;
            var res = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < decoder.Length; i++)
            {
                if ((run & options) == run) res.Add(decoder[i]);
                run = (QueryOptions)((uint)run << 1);
            }
            return res;
        }
        public static IEnumerable<KeyValuePair<string, string>> GroupOptionsToEnum(GroupingOptions options)
        {
            GroupingOptions run = (GroupingOptions)1;
            var res = new List<KeyValuePair<string, string>>();
            res.Add(groupDecoder[0]);
            for (int i = 1; i < groupDecoder.Length; i++)
            {
                if ((run & options) == run) res.Add(groupDecoder[i]);
                run = (GroupingOptions)((uint)run << 1);
            }
            return res;
        }

    }
}
