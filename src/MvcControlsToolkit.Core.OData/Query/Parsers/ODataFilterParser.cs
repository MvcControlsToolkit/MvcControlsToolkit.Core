using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.UriParser;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.OData.Parsers
{
    public class ODataFilterParser: ODataParserBase
    {
        private FilterClause filter;

        public ODataFilterParser(FilterClause x)
        {
            filter = x;
        }
        public QueryFilterBooleanOperator Parse()
        {

            var res = ParseRec(filter.Expression);
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
                    case BinaryOperatorKind.Equal:
                        return BuildComparison(binaryOperator.Left, binaryOperator.Right, "eq", "ne");
                    case BinaryOperatorKind.NotEqual:
                        return BuildComparison(binaryOperator.Left, binaryOperator.Right, "ne", "eq");
                    case BinaryOperatorKind.GreaterThan:
                        return BuildComparison(binaryOperator.Left, binaryOperator.Right, "gt", "le");
                    case BinaryOperatorKind.LessThanOrEqual:
                        return BuildComparison(binaryOperator.Left, binaryOperator.Right, "le", "gt");
                    case BinaryOperatorKind.LessThan:
                        return BuildComparison(binaryOperator.Left, binaryOperator.Right, "lt", "ge");
                    case BinaryOperatorKind.GreaterThanOrEqual:
                        return BuildComparison(binaryOperator.Left, binaryOperator.Right, "ge", "lt");
                    default:
                        return null;


                }


            }
            else if(node.Kind == QueryNodeKind.UnaryOperator)
            {
                var unaryOperator = node as UnaryOperatorNode;
                if (unaryOperator.OperatorKind == UnaryOperatorKind.Not)
                    return new QueryFilterBooleanOperator(ParseRec(unaryOperator.Operand), null)
                    { Operator = QueryFilterBooleanOperator.not };
                else return null;
            }
            else if (node.Kind == QueryNodeKind.SingleValueFunctionCall)
            {
                var functionCall = node as SingleValueFunctionCallNode;
                if (functionCall.Name == null || functionCall.Parameters == null) return null;
                var args = functionCall.Parameters.ToList();
                if (args.Count != 2) return null;
                switch (functionCall.Name.ToLower())
                {
                    case "contains":
                        return BuildComparison(args[0], args[1], "contains", null);
                    case "startswith":
                        return BuildComparison(args[0], args[1], "startswith", null);
                    case "endswith":
                        return BuildComparison(args[0], args[1], "endswith", null);
                    default:
                        return null;
                }
            }
            else return null;
        } 
        private object convertValue(object value, out short dateTimeType)
        {
            dateTimeType = 0;
            if (value == null) return null;
            if (value is Microsoft.OData.Edm.Date) 
            {
                Microsoft.OData.Edm.Date dt = (Microsoft.OData.Edm.Date)value;
                value = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);
                dateTimeType = QueryFilterCondition.IsDate;


            }
            else if (value is  Microsoft.OData.Edm.TimeOfDay)
            {
                value = (TimeSpan)value;
                dateTimeType = QueryFilterCondition.IsTime;
            }
            else if (value is DateTimeOffset || value is DateTime)
            {
                dateTimeType = QueryFilterCondition.IsDateTime;
            }
            else if (value is TimeSpan)
            {
                dateTimeType = QueryFilterCondition.IsDuration;
            }
            return value;
        }
        private QueryFilterCondition BuildComparison(Microsoft.OData.UriParser.QueryNode left, Microsoft.OData.UriParser.QueryNode right, string normalOperator, string inverseOperator)
        {
            bool inv = false;
            string propertyName = null;
            object value = null;
            short dateTimeType;
            if (left.Kind == QueryNodeKind.Constant)
            {
                inv = true;
                value = convertValue((left as ConstantNode).Value, out dateTimeType);
                if (value == null) return null;
                if (right.Kind == QueryNodeKind.SingleValuePropertyAccess)
                    propertyName = buildPropertyAccess(right as SingleValuePropertyAccessNode);
                else return null;
            }
            else if (right.Kind == QueryNodeKind.Constant)
            {
                value = convertValue((right as ConstantNode).Value, out dateTimeType);
                if (value == null) return null;
                if (left.Kind == QueryNodeKind.SingleValuePropertyAccess)
                    propertyName = buildPropertyAccess(left as SingleValuePropertyAccessNode);
                else return null;
            }
            else return null;
            if (propertyName == null) return null;
            return new QueryFilterCondition()
            {
                Operator= inv && inverseOperator != null ? inverseOperator : normalOperator,
                Property=propertyName,
                Value=value,
                DateTimeType=dateTimeType,
                Inv = inv && inverseOperator == null
            };
        }
        
        
    }
}
