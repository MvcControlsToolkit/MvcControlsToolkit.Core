using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.UriParser;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData.Parsers
{
    public class ODataSearchParser
    {
        private SearchClause search;
        public ODataSearchParser(SearchClause x)
        {
            search = x;
        }
        public QueryFilterBooleanOperator Parse()
        {

            var res = ParseRec(search.Expression);
            if (res == null) return null;
            if (!(res is QueryFilterBooleanOperator))
                return new QueryFilterBooleanOperator(res, null);
            return res as QueryFilterBooleanOperator;
        }
        private QueryFilterClause ParseRec(Microsoft.OData.UriParser.QueryNode node)
        {
            if (node.Kind == QueryNodeKind.BinaryOperator)
            {
                var binaryOperator = node as BinaryOperatorNode;
                switch (binaryOperator.OperatorKind)
                {
                    case BinaryOperatorKind.And:
                        return new QueryFilterBooleanOperator(ParseRec(binaryOperator.Left), ParseRec(binaryOperator.Right))
                        { Operator = QueryFilterBooleanOperator.and };
                    case BinaryOperatorKind.Or:
                        return new QueryFilterBooleanOperator(ParseRec(binaryOperator.Left), ParseRec(binaryOperator.Right))
                        { Operator = QueryFilterBooleanOperator.or };
                    default:
                        return null;
                }
            }
            else if (node.Kind == QueryNodeKind.UnaryOperator)
            {
                var unaryOperator = node as UnaryOperatorNode;
                if (unaryOperator.OperatorKind == UnaryOperatorKind.Not)
                    return new QueryFilterBooleanOperator(ParseRec(unaryOperator.Operand), null)
                    { Operator = QueryFilterBooleanOperator.not };
                else return null;
            }
            else if (node.Kind == QueryNodeKind.SearchTerm)
            {
                return new QueryFilterCondition()
                {
                    Operator = null,
                    Property = null,
                    Value = (node as SearchTermNode).Text
                };
               
            }
            else return null;
        }
    }
}
