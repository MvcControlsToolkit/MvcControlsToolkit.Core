using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData.Parsers
{
    public class ODataGroupingParser: ODataParserBase
    {
        private ApplyClause node;
        public ODataGroupingParser(ApplyClause x)
        {
            node = x;
        }
        private short getType(PropertyInfo property)
        {
            short dateTimeType = 0;
            var propertyType = property.PropertyType;
            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            DataTypeAttribute da = property.GetCustomAttribute(typeof(DataTypeAttribute)) as DataTypeAttribute;
            
            if (propertyType == typeof(DateTime) )
            {
                if (da?.DataType == DataType.Date) dateTimeType = QueryFilterCondition.IsDate;
                else dateTimeType = QueryFilterCondition.IsDateTime;
            }
            else if (propertyType == typeof(Week) || propertyType == typeof(Month))
            {
                
                dateTimeType = QueryFilterCondition.IsDate;
            }
            else if (propertyType == typeof(DateTimeOffset))
            {
                dateTimeType = QueryFilterCondition.IsDateTime;
            }
            else if (propertyType == typeof(TimeSpan))
            {

                if(da?.DataType == DataType.Time) dateTimeType = QueryFilterCondition.IsTime;
                else dateTimeType = QueryFilterCondition.IsDuration;
            }
            return dateTimeType;
        }
        public QueryGrouping Parse()
        {
            GroupByTransformationNode gnode = null;
            foreach (var x in node.Transformations)
            {
                
                if (x.Kind == TransformationNodeKind.GroupBy)
                {
                    gnode = x as GroupByTransformationNode;
                    break;
                }
                
            }
            if (gnode == null) return null;
            List<string> properties = null;
            List<short> types = null;
            foreach(var x in gnode.GroupingProperties)
            {
                PropertyInfo lastProperty;
                var property = buildPropertyAccess(x.Expression as SingleValuePropertyAccessNode, out lastProperty);
                if (property == null) continue;
                if (properties == null) properties = new List<string>();
                if (types == null) types = new List<short>();
                properties.Add(property);
                types.Add(getType(lastProperty));
            }
            if (properties == null) return null;
            var aggregations = new List<QueryAggregation>();
            QueryGrouping result = new QueryGrouping
            {
                Keys = properties,
                Aggregations = aggregations,
                DateTimeTypes= types
            };
            if (gnode.ChildTransformations == null || gnode.ChildTransformations.Kind != TransformationNodeKind.Aggregate) return result;
            foreach(var x in (gnode.ChildTransformations as AggregateTransformationNode).Expressions)
            {
                if (x.Expression == null && x.Method != AggregationMethod.CountDistinct) continue;
                string property = null;
                if(x.Expression != null)
                {
                    property = buildPropertyAccess(x.Expression as SingleValuePropertyAccessNode);
                    if (property == null) continue;
                }
                aggregations.Add(new QueryAggregation
                {
                    Operator= getTransformation(x.Method),
                    Property= property,
                    Alias=x.Alias,
                    IsCount= x.Method == AggregationMethod.CountDistinct
                });
            }
            return result;
        }
        private string getTransformation(AggregationMethod m)
        {
            switch(m)
            {
                case AggregationMethod.Average: return "average";
                case AggregationMethod.CountDistinct: return "countdistinct";
                case AggregationMethod.Sum: return "sum";
                case AggregationMethod.Min: return "min";
                default: return "max";
                
            }
        }
    }
}
