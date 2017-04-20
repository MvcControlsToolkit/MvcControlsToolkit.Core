using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MvcControlsToolkit.Core.Linq;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations.Queries;
using System.Text;
using System.Globalization;

namespace MvcControlsToolkit.Core.Views
{

    public abstract class QueryDescription
    {
        private const string filterName = "$filter";
        private const string applyName = "$apply";
        private const string sortingName = "$orderby";
        private const string searchName = "$search";
        private const string topName = "$top";
        private const string skipName = "$skip";
        protected  Func<string, string> UrlEncode =
            System.Net.WebUtility.UrlEncode;
        private QueryFilterBooleanOperator _Filter;
        public QueryFilterBooleanOperator Filter { get { return _Filter; } set {_Filter=value; ClearFilterCache(); } }
        private QuerySearch _Search;
        public QuerySearch Search { get { return _Search; } set { _Search = value; ClearSearchCache(); } }

        private ICollection<QuerySortingCondition> _Sorting;
        public ICollection<QuerySortingCondition> Sorting { get { return _Sorting; } set { _Sorting = value; ClearSortingCache(); } }
        private QueryGrouping _Grouping;
        public QueryGrouping Grouping { get { return _Grouping; } set { _Grouping = value; ClearGroupingCache(); } }
        public long Skip { get; set; }

        public long? Take { get; set; }

        public long Page { get; set; }

        public string EncodeSorting()
        {
            if (Sorting == null || Sorting.Count == 0) return null;
            if (Sorting.Count == 1) return Sorting.First().ToString();
            StringBuilder sb = new StringBuilder();
            foreach (var c in Sorting)
            {
                if (c == null) continue;
                if (sb.Length > 0) sb.Append(",");
                sb.Append(c.ToString());
            }
            return sb.Length>0 ? sb.ToString() : null;
             
        }
        public string EncodeFilter()
        {
            if (Filter == null) return null;
            return Filter.ToString();
        }
        public string EncodeSearch()
        {
            if (Search == null) return null;
            return Search.ToString();
        }
        public string EncodeGrouping()
        {
            if (Grouping == null) return null;
            return Grouping.ToString();
        }
        public string QueryString(bool disablePaging=false)
        {
            StringBuilder sb = new StringBuilder();
            string search = EncodeSearch();
            string filter = null;
            if(search != null)
            {
                sb.Append(searchName);
                sb.Append("=");
                sb.Append(UrlEncode(search));
            }
            else
            {
                filter = EncodeFilter();
                if(filter != null)
                {
                    sb.Append(filterName);
                    sb.Append("=");
                    sb.Append(UrlEncode(filter));
                }
            }
            string apply = EncodeGrouping();
            if(apply != null)
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(applyName);
                sb.Append("=");
                sb.Append(UrlEncode(apply));
            }
            string sorting = EncodeSorting();
            if (sorting != null)
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(sortingName);
                sb.Append("=");
                sb.Append(UrlEncode(sorting));
            }
            if (Skip > 0 && !disablePaging)
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(skipName);
                sb.Append("=");
                sb.Append(Skip.ToString(CultureInfo.InvariantCulture));
            }
            if (Take.HasValue && !disablePaging)
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(topName);
                sb.Append("=");
                sb.Append(Take.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (sb.Length > 0) return sb.ToString();
            else return null;
        }
        public override string ToString()
        {
            return AddToUrl(AttachedTo?.BaseUrl);
        }
        public string AddToUrl(string url, bool disablePaging=false)
        {
            if (url == null) url = string.Empty;
            var query = QueryString(disablePaging);
            if (string.IsNullOrWhiteSpace(query)) return url;
            if (url.Contains("?")) return url + "&" + query;
            else return url + "?" + query;
        }
        public  QueryDescription Clone()
        {
            return CloneInternal();
        }

        protected abstract QueryDescription CloneInternal();
        protected abstract void ClearFilterCache();
        protected abstract void ClearSearchCache();
        protected abstract void ClearGroupingCache();
        protected abstract void ClearSortingCache();
        private static MethodInfo grouper;
        static QueryDescription()
        {
            grouper = typeof(QueryDescription).GetMethod("StandardTGrouping", BindingFlags.Static|BindingFlags.NonPublic);
        }
        protected static IQueryable<F> StandardTGrouping<T, F, G>(IQueryable<T> cQuery, Expression<Func<T, G>> keys, Expression<Func<IGrouping<G, T>, F>> aggregations)
        {
            return cQuery.GroupBy(keys).Select(aggregations);
        }
        protected static IQueryable<F> StandardGrouping<T, F>(IQueryable<T> cQuery, LambdaExpression keys, LambdaExpression aggregations)
        {
            var mt=grouper.MakeGenericMethod(typeof(T), typeof(F), keys.Type.GetGenericArguments()[1]);
            return mt.Invoke(null, new object[] { cQuery, keys, aggregations }) as IQueryable<F>;

        }
        public Endpoint AttachedTo { get; set; }
        public void AttachEndpoint(string baseUrl, bool returnsjSon=false, string bearerToken=null, string ajaxId=null)
        {
            AttachedTo = new Endpoint
            {
                BaseUrl = baseUrl,
                ReturnsJson = returnsjSon,
                BearerToken = bearerToken,
                Verb = Endpoint.Get,
                AjaxId=ajaxId
            };
        }

        public bool CompatibleProperty(string propertyName)
        {
            if (Grouping == null) return true;
            else return Grouping.CompatibleProperty(propertyName);
        }
        
    }


    public class QueryDescription<T> : QueryDescription
    {

        private Expression<Func<T, bool>> filterCache;
        private Func<IQueryable<T>, IOrderedQueryable<T>> sortingCache;
        private Func<IQueryable<T>, IQueryable<T>> groupingCache;
        private MethodInfo grouper;

        public bool SearchAllowed()
        {
            if (Search == null) return false;
            return Search.Allowed(typeof(T));
        }

        public Expression<Func<T, bool>> GetFilterExpression()
        {
            if (Filter == null) return null;
            if (filterCache != null) return filterCache;
            var par = Expression.Parameter(typeof(T), "m");
            return filterCache = Expression.Lambda(Filter.BuildExpression(par, typeof(T)), par) as Expression<Func<T, bool>>;

        }
        public Func<IQueryable<T>, IQueryable<T>> GetGrouping()
        {
            if (groupingCache != null) return groupingCache;
            return groupingCache = GetGrouping<T>();
        }

        public Func<IQueryable<T>, IQueryable<F>> GetGrouping<F>()
        {
            if (Grouping == null)
            {
                groupingCache = null;
                return null;
            }
            PropertyInfo[] properties;
            var keys = Grouping.BuildGroupingExpression<T>(out properties);
            if (keys == null) return null;
            var aggregations = Grouping.GetProjectionExpression<T, F>(properties);
            return q =>
            {
                return StandardGrouping<T, F>(q, keys, aggregations);
            };
        }
        public Func<IQueryable<F>, IOrderedQueryable<F>> GetSorting<F>()
        {
            if (Sorting == null || Sorting.Count == 0)
            {
                sortingCache = null;
                return null;
            }
            return (q) =>
           {
               bool start = true;
               IOrderedQueryable<F> result = null;
               foreach (var s in Sorting)
               {
                   if (start)
                   {
                       start = false;
                       if (s.Down) result = q.OrderByDescending(s.GetSortingLambda(typeof(F)));
                       else result = q.OrderBy(s.GetSortingLambda(typeof(F)));
                   }
                   else
                   {
                       if (s.Down) result = result.ThenByDescending(s.GetSortingLambda(typeof(T)));
                       else result = result.ThenBy(s.GetSortingLambda(typeof(T)));
                   }
               }
               return result;
           };
        }
        public Func<IQueryable<T>, IOrderedQueryable<T>> GetSorting()
        {
            if (sortingCache != null) return sortingCache;
            return sortingCache = GetSorting<T>();
        }
        protected override QueryDescription CloneInternal()
        {
            return MemberwiseClone() as QueryDescription;
        }
        protected override void ClearFilterCache()
        {
            filterCache = null;
        }
        protected override void ClearGroupingCache()
        {
            groupingCache=null;
        }
        protected override void ClearSearchCache()
        {
           
        }
        protected override void ClearSortingCache()
        {
            sortingCache = null; ;
        }
        public new QueryDescription<T> Clone()
        {
            return MemberwiseClone() as QueryDescription<T>;
        }
        
        public void AddFilterCondition(Expression<Func<T, bool>> filter, bool useOr=false)
        {
            if (filter == null) return;
            var res = QueryFilterClause.FromLinQExpression(filter);
            if (res == null) return;
            if (Filter == null)
            {
                Filter = res is QueryFilterBooleanOperator ? res as QueryFilterBooleanOperator
                      : new QueryFilterBooleanOperator(res, null);
                return;
            }

            QueryFilterClause cleanFilter;
            if (Filter.Operator != QueryFilterBooleanOperator.not)
            {
                if (Filter.Child1 == null && Filter.Argument1 == null) cleanFilter = Filter.Argument2 as QueryFilterClause ?? Filter.Child2;
                else if (Filter.Child2 == null && Filter.Argument2 == null) cleanFilter = Filter.Argument1 as QueryFilterClause ?? Filter.Child1;
                else cleanFilter = Filter;
            }
            else cleanFilter = Filter;
            Filter = new QueryFilterBooleanOperator(cleanFilter, res);
            if (useOr) Filter.Operator = QueryFilterBooleanOperator.or;    

        }
         public void CustomUrlEncode(Func<string, string> fun)
        {
            UrlEncode = fun ?? UrlEncode;
        }
        public IDictionary<string, QueryFilterCondition> PopulateFilterModel(T model)
        {
            if (model == null || Filter == null) return null;
            var res = new Dictionary<string, QueryFilterCondition>();
            Filter.PopulateFilterModel<T>(model, res);
            return res;

        }
    }
}
