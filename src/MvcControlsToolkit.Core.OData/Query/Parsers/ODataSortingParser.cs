using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.UriParser;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData.Parsers
{
    public class ODataSortingParser: ODataParserBase
    {
        private OrderByClause node;
        public ODataSortingParser(OrderByClause x)
        {
            node = x;
        }
        public ICollection<QuerySortingCondition> Parse()
        {
            var res = new List<QuerySortingCondition>();
            ParseRec(res, node);
            return res;
        }
        private void ParseRec(List<QuerySortingCondition> x, OrderByClause node)
        {
            string path = buildPropertyAccess(node.Expression as SingleValuePropertyAccessNode);
            if (path == null) return;
            x.Add(new QuerySortingCondition { Property = path, Down = node.Direction == OrderByDirection.Descending });
            if (node.ThenBy != null) ParseRec(x, node.ThenBy);
        }
    }
}
