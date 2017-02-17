using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using MvcControlsToolkit.Core.Options.Attributes;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;
using MvcControlsToolkit.Core.OData.Parsers;




namespace MvcControlsToolkit.Core.OData
{
    public class ODataQueryProvider : IWebQueryProvider
    {
        #region types mapping
        private static IDictionary<Type, EdmPrimitiveTypeKind> typeMapping =
            new Dictionary<Type, EdmPrimitiveTypeKind>()
            {
                {typeof(byte[]), EdmPrimitiveTypeKind.Binary},
                {typeof(bool), EdmPrimitiveTypeKind.Boolean},
                {typeof(byte), EdmPrimitiveTypeKind.Byte},
                {typeof(sbyte), EdmPrimitiveTypeKind.Byte},
                {typeof(DateTime), EdmPrimitiveTypeKind.Date},
                {typeof(Month), EdmPrimitiveTypeKind.Date},
                {typeof(Week), EdmPrimitiveTypeKind.Date},
                {typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset},
                {typeof(decimal), EdmPrimitiveTypeKind.Decimal},
                {typeof(double), EdmPrimitiveTypeKind.Double},
                {typeof(TimeSpan), EdmPrimitiveTypeKind.Duration},
                {typeof(Guid), EdmPrimitiveTypeKind.Guid},
                {typeof(short), EdmPrimitiveTypeKind.Int16},
                {typeof(ushort), EdmPrimitiveTypeKind.Int16},
                {typeof(int), EdmPrimitiveTypeKind.Int32},
                {typeof(long), EdmPrimitiveTypeKind.Int64},
                {typeof(uint), EdmPrimitiveTypeKind.Int32},
                {typeof(ulong), EdmPrimitiveTypeKind.Int64},
                {typeof(float), EdmPrimitiveTypeKind.Single},
                {typeof(string), EdmPrimitiveTypeKind.String}
            };
        private static EdmPrimitiveTypeKind? GetPrimitiveEDMType(PropertyInfo p, Type pType)
        {
            
            
            if (pType == typeof(DateTime))
            {
                var att = p.GetCustomAttribute(typeof(DataTypeAttribute)) as DataTypeAttribute;
                if (att == null) return EdmPrimitiveTypeKind.DateTimeOffset;
                if (att.DataType != DataType.Date) return EdmPrimitiveTypeKind.DateTimeOffset;
                return EdmPrimitiveTypeKind.Date; 
            }
            else if (pType == typeof(TimeSpan))
            {
                var att = p.GetCustomAttribute(typeof(DataTypeAttribute)) as DataTypeAttribute;
                if (att == null) return EdmPrimitiveTypeKind.Duration;
                if (att.DataType != DataType.Time) return EdmPrimitiveTypeKind.Duration;
                return EdmPrimitiveTypeKind.TimeOfDay;
            }
            EdmPrimitiveTypeKind result;
            if (typeMapping.TryGetValue(pType, out result)) return result;
            else return null;
        }
        #endregion
        #region EDM building
        private static EdmClrType GetEdmType(Type type)
        {
            var result = new EdmClrType(type);
            foreach (var prop in type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var ptype = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if(TypeDescriptor.GetConverter(ptype).CanConvertFrom(typeof(string)))
                {
                    var edmType = GetPrimitiveEDMType(prop, ptype);
                    if (edmType.HasValue) result.AddProperty(new EdmClrProperty(result, prop, edmType.Value));
                }
                else if(!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ptype))
                {
                    result.AddProperty(new EdmClrProperty(result, prop, new EdmClrType(ptype)));
                }
                
            }
            return result;
        }
        private static EdmModel GetEdmModel(Type type)
        {
            var result = new EdmModel();
            var container = new EdmEntityContainer("Default", "Container");
            container.AddEntitySet("Entities", GetEdmType(type));
            result.AddElement(container);
            return result;
        }
        private static ConcurrentDictionary<Type, EdmModel> models =
            new ConcurrentDictionary<Type, EdmModel>();
        private static EdmModel GetCachedEdmModel(Type type)
        {
            EdmModel result;
            if (models.TryGetValue(type, out result)) return result;
            result = GetEdmModel(type);
            models.TryAdd(type, result);
            return result;
        }
        #endregion
        [OptionName("$filter")]
        public string Filter { get; set; }
        [OptionName("$orderby")]
        public string OrderBy { get; set; }
        [OptionName("$apply")]
        public string Apply { get; set; }
        [OptionName("$search")]
        public string Search { get; set; }
        [OptionName("$skip")]
        public string Skip { get; set; }
        [OptionName("$top")]
        public string Top { get; set; }
        private IDictionary<string, string> _dictionary;
        protected IDictionary<string, string> Dictionary { get {
                if (_dictionary != null) return _dictionary;
                _dictionary = new Dictionary<string, string>()
                {
                    {"$filter", Filter },
                    {"$orderby", OrderBy },
                    {"$apply", Apply },
                    {"$search", Search },
                    {"$skip", Skip },
                    {"$top", Top },

                };
                return _dictionary;
        } }
        protected QueryFilterBooleanOperator ParseFilter(FilterClause x)
        {
            if (x == null) return null;
            return new ODataFilterParser(x).Parse(); ;
        }
        protected ICollection<QuerySortingCondition> ParseOrderBy(OrderByClause x)
        {
            if (x == null) return null;
            return new ODataSortingParser(x).Parse();
        }
        protected QuerySearch ParseSearch(SearchClause x)
        {
            if (x == null) return null;
            var res = new ODataSearchParser(x).Parse();
            if (res == null) return null;
            else return new QuerySearch {Value = res };
        }
        protected QueryGrouping ParseApply(ApplyClause x)
        {
            if (x == null) return null;
            return new ODataGroupingParser(x).Parse();
            
        }
        public QueryDescription<T> Parse<T>()
        {
            var model = GetCachedEdmModel(typeof(T));
            var entities = model.FindDeclaredNavigationSource("Entities");
            var parser = new ODataQueryOptionParser(model, entities.EntityType(),
                entities, Dictionary);

            var filter = Filter == null ? null : parser.ParseFilter();
            var orderby = OrderBy == null ? null : parser.ParseOrderBy();
            var search = Search == null ? null : parser.ParseSearch();
            var apply = Apply == null ? null : parser.ParseApply();
            
            var result = new QueryDescription<T>()
            {
                Skip = Skip == null ? 0 : parser.ParseSkip()??0,
                Take = Top == null ? null : parser.ParseTop(),
                Filter = ParseFilter(filter),
                Sorting = ParseOrderBy(orderby),
                Search = ParseSearch(search),
                Grouping = ParseApply(apply)
            };
            if (result.Take == null) result.Page = result.Skip == 0 ? 1 : 2;
            else
            {
                result.Page = (result.Skip / result.Take.Value)+1;
                if (result.Skip % result.Take.Value > 0) result.Page++;
            } 
            return result;
        }
        

    }
}
