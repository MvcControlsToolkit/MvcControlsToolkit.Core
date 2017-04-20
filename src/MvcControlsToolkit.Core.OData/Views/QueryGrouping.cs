using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations.Queries;
using MvcControlsToolkit.Core.OData.Utilities;

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

            if (Property == null || Operator == null || Alias == null) return null;
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
        private HashSet<string> propertySet;
        private bool setComputed;
        private void buildHash()
        {
            setComputed = true; ;
            if (Keys == null || Keys.Count == 0) return;
            propertySet = new HashSet<string>(Keys);
            if (Aggregations != null)
                propertySet.UnionWith(Aggregations.Select(m => m.Alias));
        }
        public bool CompatibleProperty(string propertyName)
        {
            if (propertyName == null) return false;
            if (!setComputed) buildHash();
            if (propertySet == null) return true;
            else return propertySet.Contains(propertyName);
        }
        internal LambdaExpression BuildGroupingExpression<T>(out PropertyInfo[]  properties)
        {
            properties = null;
            if (Keys == null || Keys.Count == 0) return null;
            if (Keys.Count > 32) throw new NotSupportedException("grouping is supported up to 32 properties");
            var t = typeof(T);
            var par = Expression.Parameter(t, "m");
            var assignements = new List<MemberAssignment>();
            
            MemberExpression[] members = new MemberExpression[Keys.Count];
            Type[] types = new Type[Keys.Count];
            int i = 0;
            foreach (var key in Keys)
            {
                var access = members[i] = BuildAccess(key, par, t, QueryOptions.GroupBy, "groupby") as MemberExpression;
                types[i] = (access.Member as PropertyInfo).PropertyType;
                i++;
            }
            i = 0;
            int n;
            properties = AnonymousTypesFarm.GetProperties(types, out n);
            Expression[] fmembers = members as Expression[];
            if (n > members.Length)
                fmembers = fmembers.Concat(Enumerable.Repeat(Expression.Constant(0), n - members.Length)).ToArray();
            
            return Expression.Lambda(Expression.New(properties[0].DeclaringType.GetConstructors().Single(), fmembers), par);
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

            if (t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
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
        internal LambdaExpression GetProjectionExpression<T, F>(PropertyInfo[] properties)
        {
            
            var assignements = new List<MemberAssignment>();
            Type iType = properties[0].DeclaringType;
            var t = typeof(T);
            var f = typeof(F);
            var g = typeof(IGrouping<,>).MakeGenericType(iType, typeof(T));
            var par = Expression.Parameter(g, "m");
            int i = 0;
            var keyProp = g.GetProperty("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach (var key in Keys)
            {
                var prop = properties[i];
                var access =
                    Expression.Property(Expression.Property(par, keyProp), prop);
                
                var alias = QueryNodeCache.GetPath(f, key);
                if (alias.Item1.Count > 1) throw new NestedPropertyNotAllowedException(key);
                assignements.Add(Expression.Bind(alias.Item1[0], access));
                
                i++;
            }
            if (Aggregations != null && Aggregations.Count > 0)
            {
                foreach (var agg in Aggregations)
                {
                    var innerPar = Expression.Parameter(t, "n");
                    Expression access = BuildAccess(agg.Property, innerPar, t, null, "aggregate");
                    var alias = QueryNodeCache.GetPath(f, agg.Alias);
                    if (alias.Item1.Count > 1) throw new NestedPropertyNotAllowedException(agg.Alias);
                    Expression call;
                    LambdaExpression argExpression = Expression.Lambda(access, innerPar);
                    if (agg.IsCount)
                    {
                        var pType = access.Type;
                        call = BuildCall(getAggregationName(agg.Operator, alias.Item1[0]),
                                BuildCall("Distinct",
                                    BuildCall("Select", par, g, argExpression),
                                pType, null),
                            pType, null);

                    }
                    else
                        call = BuildCall(getAggregationName(agg.Operator, alias.Item1[0]), par, g, argExpression);
                    var dType = alias.Item1[0].PropertyType;
                    if (dType == call.Type)
                        assignements.Add(Expression.Bind(alias.Item1[0], call));
                    else
                        assignements.Add(Expression.Bind(alias.Item1[0], Expression.Convert(call, dType)));
                }
            }
            return Expression.Lambda(Expression.MemberInit(Expression.New(f), assignements), par) ;
        }
        private string encodeGroups()
        {
            if (Keys == null || Keys.Count == 0) return null;
            if (Keys.Count == 1) return EncodeProperty(Keys.First());
            StringBuilder sb = new StringBuilder();

            foreach (var key in Keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
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
                var sagg = agg.ToString();
                if (sagg == null) continue;
                if (sb.Length > 0) sb.Append(",");
                sb.Append(sagg);
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            var groups = encodeGroups();
            if (string.IsNullOrEmpty(groups)) return null;

            var aggs = encodeAggrgates();
            if (string.IsNullOrEmpty(aggs)) return string.Format("groupby(({0}))", groups);
            else return string.Format("groupby(({0}),aggregate({1}))", groups, aggs);
        }
    }
}
