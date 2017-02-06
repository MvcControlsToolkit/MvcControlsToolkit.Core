using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
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
            foreach(var x in gnode.GroupingProperties)
            {
                var property = buildPropertyAccess(x.Expression as SingleValuePropertyAccessNode);
                if (property == null) continue;
                if (properties == null) properties = new List<string>();
                properties.Add(property);
            }
            if (properties == null) return null;
            var aggregations = new List<QueryAggregation>();
            QueryGrouping result = new QueryGrouping
            {
                Keys = properties,
                Aggregations = aggregations
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
