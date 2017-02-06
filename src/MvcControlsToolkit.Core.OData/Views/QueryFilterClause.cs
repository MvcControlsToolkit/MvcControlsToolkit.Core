using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.Views
{
    public abstract class QueryFilterClause : QueryNode
    {
        internal abstract Expression BuildExpression(ParameterExpression par, Type t);
        protected static QueryFilterClause FromLinQExpressionBody(Expression body)
        {
            if(body == null) return null;
            return
                QueryFilterBooleanOperator.fromBooleanExpression(body) as QueryFilterClause ??
                QueryFilterCondition.fromConditionExpression(body);

        }
        public static QueryFilterClause FromLinQExpression<T>(Expression<Func<T, bool>> filter)
        {
            if (filter == null) return null;
            return FromLinQExpressionBody(filter.Body);
        }
    }
    public class QueryFilterBooleanOperator : QueryFilterClause
    {
        public const int and = 0;
        public const int or = 1;
        public const int not = 2;
        public QueryFilterBooleanOperator()
        {
        }
        internal static QueryFilterBooleanOperator fromBooleanExpression(Expression booleanExpression)
        {
            switch (booleanExpression.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.Or:
                    var bop = booleanExpression as BinaryExpression;
                    var res = new QueryFilterBooleanOperator(FromLinQExpressionBody(bop.Left), FromLinQExpressionBody(bop.Right));
                    if (booleanExpression.NodeType == ExpressionType.Or) res.Operator = QueryFilterBooleanOperator.or;
                    return res;
                case ExpressionType.Not:
                    var uop = booleanExpression as UnaryExpression;
                    var ures = new QueryFilterBooleanOperator(FromLinQExpressionBody(uop.Operand), null);
                    ures.Operator = QueryFilterBooleanOperator.not;
                    return ures;
                default: return null;
            }
        }
        public QueryFilterBooleanOperator(QueryFilterClause arg1, QueryFilterClause arg2)
        {
            if (arg1 is QueryFilterBooleanOperator) Child1 = arg1 as QueryFilterBooleanOperator;
            else Argument1 = arg1 as QueryFilterCondition;
            if (arg2 is QueryFilterBooleanOperator) Child2 = arg2 as QueryFilterBooleanOperator;
            else Argument2 = arg2 as QueryFilterCondition;
        }
        public int Operator { get; set; }

        public QueryFilterCondition Argument1 { get; set; }
        public QueryFilterCondition Argument2 { get; set; }
        
        public QueryFilterBooleanOperator Child1 { get; set; }
        public QueryFilterBooleanOperator Child2 { get; set; }
        private QueryFilterClause arg1 { get { return (QueryFilterClause)Argument1 ?? (QueryFilterClause)Child1; } }
        private QueryFilterClause arg2 { get { return (QueryFilterClause)Argument2 ?? (QueryFilterClause)Child2; } }
        internal override Expression BuildExpression(ParameterExpression par, Type t)
        {

            if(Operator == not)
            {
                var arg = arg1 ?? arg2;
                if (arg == null) return null;
                else return Expression.Not(BuildExpression(par, t));
            }
            else if (arg1 == null)
            {
                if (arg2 == null) return null;
                else return arg1.BuildExpression(par, t);
            }
            else if (arg2 == null) 
                return arg2.BuildExpression(par, t);
            else if (Operator==or)
                return Expression.Or(arg1.BuildExpression(par, t), arg2.BuildExpression(par, t));
            else
                return Expression.And(arg1.BuildExpression(par, t), arg2.BuildExpression(par, t));
        }
        public override string ToString()
        {
            if (Operator == not) return string.Format("(not {0})", (arg1 ?? arg2).ToString());
            else if (arg1 == null) return arg2.ToString();
            else if (arg2 == null) return arg1.ToString();
            var sarg1 = arg1.ToString();
            var sarg2 = arg2.ToString();
            if (sarg1 == null) return sarg2;
            if (sarg2 == null) return sarg1;
            else if (Operator == and) return string.Format("({0} and {1})", sarg1, sarg2.ToString());
            else return string.Format("({0} or {1})", sarg1.ToString(), sarg2.ToString());
        }
        
    }
    public class QueryFilterCondition : QueryFilterClause
    {
        private static MethodInfo startsWithMethod = typeof(string).GetTypeInfo().GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        private static MethodInfo containsWithMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

        public const short IsNotDateTime= 0;
        public const short IsDate = 1;
        public const short IsTime = 2;
        public const short IsDateTime = 3;
        public const short IsDuration = 4;

        private static Expression removeConvert(Expression exp)
        {
            while (exp != null && exp.NodeType == ExpressionType.Convert) exp = (exp as UnaryExpression).Operand;
            return exp;
        }
        private static string getPropertyName(Expression exp)
        {
            StringBuilder sb = null;
            while(exp != null && exp.NodeType == ExpressionType.MemberAccess)
            {
                var ma = exp as MemberExpression;
                if (!(ma.Member is PropertyInfo)) return null;
                if (sb == null) sb = new StringBuilder();
                sb.Insert(0, ma.Member.Name);
                exp = ma.Expression;
            }
            if (exp.NodeType == ExpressionType.Parameter) return sb == null ? null : sb.ToString();
            else return null;
        }
        
        private static QueryFilterCondition getBinaryCondition(Expression arg1, Expression arg2, string directOperator, string invOperator)
        {
            arg1 = removeConvert(arg1);
            arg2 = removeConvert(arg2);
            object value;
            string property;
            PropertyInfo prop = null;
            bool inv = false;
            if (arg1.NodeType == ExpressionType.Constant)
            {
                value = (arg1 as ConstantExpression).Value;
                property = getPropertyName(arg2);
                if (property == null) return null;
                prop = ((arg2 as MemberExpression).Member as PropertyInfo);
                inv = true;

            }
            else if (arg1.NodeType == ExpressionType.MemberAccess)
            {
                property = getPropertyName(arg1);
                if (property == null) return null;
                prop = ((arg1 as MemberExpression).Member as PropertyInfo);
                if (arg2.NodeType != ExpressionType.Constant) return null;
                value = (arg2 as ConstantExpression).Value;
            }
            else return null;
            if (value == null ) return null;
            short dateTimeType = QueryFilterCondition.IsNotDateTime;
            var type = prop.PropertyType;
            if(type == typeof(Month))
            {
                dateTimeType = QueryFilterCondition.IsDate;
                if (value is Month) value = (DateTime)value;
            }
            else if (type == typeof(Week))
            {
                dateTimeType = QueryFilterCondition.IsDate;
                if (value is Week) value = (DateTime)value;
            }
            else if (type == typeof(DateTime))
            {
                var att = prop.GetCustomAttribute(typeof(DataTypeAttribute)) as DataTypeAttribute;
                if (att != null && att.DataType == DataType.Date) dateTimeType = QueryFilterCondition.IsDate;
                else dateTimeType = QueryFilterCondition.IsDateTime;
            }
            else if (type == typeof(DateTimeOffset))
            {
                dateTimeType = QueryFilterCondition.IsDateTime;
            }
            else if(type == typeof(TimeSpan))
            {
                var att = prop.GetCustomAttribute(typeof(DataTypeAttribute)) as DataTypeAttribute;
                if (att != null && att.DataType == DataType.Time) dateTimeType = QueryFilterCondition.IsTime;
                else dateTimeType = QueryFilterCondition.IsDuration;
            }
            return new QueryFilterCondition
            {
                Inv = inv && invOperator == null,
                Operator = inv && invOperator != null ? invOperator : directOperator,
                Property = property,
                Value = value,
                DateTimeType = dateTimeType
            };
        }
        internal static QueryFilterCondition fromConditionExpression(Expression conditionExpression)
        {
            if (conditionExpression is BinaryExpression)
            {
                var binaryOperator = conditionExpression as BinaryExpression;
                if (binaryOperator == null) return null;
                switch (binaryOperator.NodeType)
                {
                    case ExpressionType.Equal:
                        return getBinaryCondition(binaryOperator.Left, binaryOperator.Right, "eq", "ne");
                    case ExpressionType.NotEqual:
                        return getBinaryCondition(binaryOperator.Left, binaryOperator.Right, "ne", "eq");
                    case ExpressionType.GreaterThan:
                        return getBinaryCondition(binaryOperator.Left, binaryOperator.Right, "gt", "le");
                    case ExpressionType.LessThanOrEqual:
                        return getBinaryCondition(binaryOperator.Left, binaryOperator.Right, "le", "gt");
                    case ExpressionType.LessThan:
                        return getBinaryCondition(binaryOperator.Left, binaryOperator.Right, "lt", "ge");
                    case ExpressionType.GreaterThanOrEqual:
                        return getBinaryCondition(binaryOperator.Left, binaryOperator.Right, "ge", "lt");
                    default:
                        return null;


                }
            }
            else if (conditionExpression is MethodCallExpression)
            {
                var methodCall = conditionExpression as MethodCallExpression;
                if (methodCall.Method == startsWithMethod)
                    return getBinaryCondition(methodCall.Object, methodCall.Arguments[0], "startswith", null);
                else if (methodCall.Method == endsWithMethod)
                    return getBinaryCondition(methodCall.Object, methodCall.Arguments[0], "endswith", null);
                else if (methodCall.Method == containsWithMethod)
                    return getBinaryCondition(methodCall.Object, methodCall.Arguments[0], "contains", null);
                else return null;
            }
            else return null;
            
        }
        internal Func<Expression, Expression, Expression> getBuilder(out QueryOptions operation)
        {
            operation = QueryOptions.None;
            Func<Expression, Expression, Expression> result = null;

            switch (Operator)
            {
                case "eq":
                    operation = QueryOptions.Equal;
                    result = (x, y) => Expression.Equal(x, y);
                    break;
                case "ne":
                    operation = QueryOptions.NotEqual;
                    result = (x, y) => Expression.NotEqual(x, y);
                    break;
                case "lt":
                    operation = QueryOptions.LessThan;
                    result = (x, y) => Expression.LessThan(x, y);
                    break;
                case "le":
                    operation = QueryOptions.LessThanOrEqual;
                    result = (x, y) => Expression.LessThanOrEqual(x, y);
                    break;
                case "gt":
                    operation = QueryOptions.GreaterThan;
                    result = (x, y) => Expression.GreaterThan(x, y);
                    break;
                case "ge":
                    operation = QueryOptions.GreaterThanOrEqual;
                    result = (x, y) => Expression.GreaterThanOrEqual(x, y);
                    break;
                case "startswith":
                    operation = QueryOptions.StartsWith;
                    if(Inv) result = (x, y) => Expression.Call(y, startsWithMethod, x);
                    else result = (x, y) => Expression.Call(x, startsWithMethod, y);
                    break;
                case "endswith":
                    operation = QueryOptions.EndsWith;
                    if (Inv) result = (x, y) => Expression.Call(y, endsWithMethod, x);
                    else result = (x, y) => Expression.Call(x, endsWithMethod, y);
                    break;
                case "contains":
                    operation = Inv ? QueryOptions.IsContainedIn : QueryOptions.Contains;
                    if (Inv) result = (x, y) => Expression.Call(y, containsWithMethod, x);
                    else result = (x, y) => Expression.Call(x, containsWithMethod, y);

                    break;
                default: result = null; break;
            }
            return result;
        }
        public override string ToString()
        {
            if (Value == null) return null;
            switch (Operator)
            {
                case null:
                    var val = Value.ToString();
                    if (val != null && val.Contains(' ')) return "\"" + val + "\"";
                    else return val;
                case "startswith":
                case "endswith": 
                case "contains":
                    var sc = encodeConstant(Value, DateTimeType);
                    if (sc == null) return null;
                    if (Inv) return string.Format("{0}({1},{2})", Operator, sc, EncodeProperty(Property));
                    else return string.Format("{0}({1},{2})", Operator, EncodeProperty(Property), sc);
                default:
                    var s = encodeConstant(Value, DateTimeType);
                    if (s == null) return null;
                    else return string.Format("({0} {1} {2})", EncodeProperty(Property), Operator, s);
                    
            }
        }
        public string Operator { get; set; }
        public string Property { get; set; }
        public object Value { get; set; }
        public bool Inv { get; set; }
        public short DateTimeType { get; set; }
        internal override Expression BuildExpression(ParameterExpression par, Type t)
        {
            QueryOptions operation;
            var builder = getBuilder(out operation);
            if (builder == null) throw new OperationNotAllowedException(Property, Operator);
            var propertyAccess = BuildAccess(Property, par, t, operation, Operator);
            var value = Value;
            var pType = Nullable.GetUnderlyingType(propertyAccess.Type) ?? propertyAccess.Type;
            if (Value != null && pType != Value.GetType())
            {
                var type = Value.GetType();
                
                
                
                if (pType == typeof(TimeSpan) && type == typeof(DateTime))
                {
                    var dt = (DateTime)Value;
                    value = dt.Subtract(dt.Date);
                }
                else if (type == typeof(DateTimeOffset) && pType == typeof(DateTime))
                {
                    var cvalue = ((DateTimeOffset)value).UtcDateTime;
                    value = new DateTime(cvalue.Year, cvalue.Month, cvalue.Day,
                        cvalue.Hour, cvalue.Minute, cvalue.Second, cvalue.Millisecond, DateTimeKind.Unspecified); ;
                }   
                else  value = Convert.ChangeType(Value, propertyAccess.Type);
            }
                
                

            if (Inv) return builder(Expression.Constant(value), propertyAccess);
            else return builder(propertyAccess, Expression.Constant(value));
        }
    }
}
