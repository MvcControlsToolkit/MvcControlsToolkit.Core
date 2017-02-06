using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations.Queries;

namespace MvcControlsToolkit.Core.Views
{
    public class QueryAggregation : QueryNode
    {
        public string Operator { get; set; }
        public string Property { get; set; }
        public bool IsCount { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            
            return string.Format("{0} with {1} as {2}", 
                EncodeProperty(Property),
                Operator,
                Alias);
        }
    }
    public class QueryGrouping : QueryNode
    {
        public ICollection<string> Keys { get; set; }
        public ICollection<QueryAggregation> Aggregations { get; set; }

        internal Expression<Func<T, T>> BuildGroupingExpression<T>()
        {
            if (Keys == null || Keys.Count == 0) return null;
            var t = typeof(T);
            var par = Expression.Parameter(t, "m");
            var assignements = new List<MemberAssignment>();
            foreach (var key in Keys)
            {
                var access = BuildAccess(key, par, t, QueryOptions.GroupBy, "groupby");
                assignements.Add(Expression.Bind((access as MemberExpression).Member, access));
            }
            return Expression.Lambda(Expression.MemberInit(Expression.New(t), assignements), par) as Expression<Func<T, T>>;
        }
        internal string getAggregationName(string x, PropertyInfo property)
        {
            x = x.ToLower();
            Type t = property.PropertyType;
            t = Nullable.GetUnderlyingType(t) ?? t;
            string res;
            if (x == "countdistinct")
            {
                if (t == typeof(int)) return "Count";
                else if (t == typeof(long)) return "LongCount";
                else throw new OperationNotAllowedException(property.Name, "count");
            }

            if (t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
            {
                if (x == "sum") res = "Sum";
                else if (x == "average") res = "Average";
                else if (x == "min") res = "Min";
                else if (x == "max") res = "Max";
                else throw new OperationNotAllowedException(property.Name, x);
            }
            else throw new OperationNotAllowedException(property.Name, x);
            return res;
        }
        internal Expression<Func<IGrouping<T, T>, F>> GetProjectionExpression<T, F>()
        {
            if (Aggregations == null || Aggregations.Count == 0) return null;
            var assignements = new List<MemberAssignment>();
            var t = typeof(T);
            var f = typeof(F);
            var g = typeof(IGrouping<T, T>);
            var par = Expression.Parameter(g, "m");
            foreach (var key in Keys)
            {
                var access = BuildAccess("Key." + key, par, g, QueryOptions.GroupBy, "groupby");
                if (t == f)
                    assignements.Add(Expression.Bind((access as MemberExpression).Member, access));
                else
                {
                    var alias = QueryNodeCache.GetPath(f, key);
                    if (alias.Item1.Count > 1) throw new NestedPropertyNotAllowedException(key);
                    assignements.Add(Expression.Bind(alias.Item1[0], access));
                }
            }
            foreach (var agg in Aggregations)
            {
                var innerPar = Expression.Parameter(t, "n");
                Expression access = agg.IsCount ? null : BuildAccess(agg.Property, innerPar, t, null, "aggregate");
                var alias = QueryNodeCache.GetPath(f, agg.Alias);
                if (alias.Item1.Count > 1) throw new NestedPropertyNotAllowedException(agg.Alias);
                var call = BuildCall(getAggregationName(agg.Operator, alias.Item1[0]), par, g, Expression.Lambda(access, innerPar));
                assignements.Add(Expression.Bind(alias.Item1[0], call));
            }
            return Expression.Lambda(Expression.MemberInit(Expression.New(t), assignements), par) as Expression<Func<IGrouping<T, T>, F>>;
        }
        private string encodeGroups()
        {
            if (Keys == null || Keys.Count == 0) return null;
            if (Keys.Count == 1) return EncodeProperty(Keys.First());
            StringBuilder sb = new StringBuilder();

            foreach (var key in Keys)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(EncodeProperty(key));
            }
            return sb.ToString();
        }
        private string encodeAggrgates()
        {
            if (Aggregations == null || Aggregations.Count == 0) return null;
            if (Aggregations.Count == 1) return Aggregations.First().ToString();
            StringBuilder sb = new StringBuilder();

            foreach (var agg in Aggregations)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(agg.ToString());
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            var groups = encodeGroups();
            if (groups == null) return null;

            var aggs = encodeAggrgates();
            if (aggs == null) return string.Format("groupby(({0}))", groups);
            else return string.Format("groupby(({0}),aggregate({1}))", groups, aggs);
        }
    }
}
