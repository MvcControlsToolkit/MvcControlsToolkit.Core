using System;
using System.Globalization;
using System.Linq.Expressions;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations.Queries;

namespace MvcControlsToolkit.Core.Views
{
    public class QueryNode
    {
        internal Expression BuildAccess(string name, ParameterExpression par, Type t, QueryOptions? operation, string operationName, bool notNested = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return par;

            Expression curr = par;
            var gres = QueryNodeCache.GetPath(t, name);
            if (notNested && gres.Item1.Count > 1) throw new NestedPropertyNotAllowedException(name);
            if (operation.HasValue && (gres.Item2 == null || gres.Item2.Allowed(operation.Value)))
                throw new OperationNotAllowedException(name, operationName);
            foreach (var property in gres.Item1)
            {
                curr = Expression.Property(curr, property);
            }
            return curr;
        }
        internal Expression BuildCall(string name, ParameterExpression par, Type t, Expression arg)
        {
            var method = QueryNodeCache.GetMethod(t, name, arg.Type);
            return Expression.Call(par, method, arg);
        }
        internal string EncodeProperty(string name)
        {
            if (name == null) return null;
            return name.Replace('.', '/');
        }
        internal string encodeConstant(object value, short dateTimeType)
        {
            if (value == null) return null;
            var type = value.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(string)) return value as string;
            if (type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(uint)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(byte)
                || type == typeof(sbyte))
                return (value as IFormattable).ToString("D", CultureInfo.InvariantCulture);
            else if (type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
            ) return (value as IFormattable).ToString("G", CultureInfo.InvariantCulture);
            if (type == typeof(string)) return "'" + value.ToString() + "'";
            if (type == typeof(Guid)) return value.ToString();
            if (type == typeof(bool)) return ((bool)value) ? "true" : "false";
            if (type == typeof(DateTime))
            {
                var dt = (DateTime)value;
                if(dateTimeType == QueryFilterCondition.IsDate)
                    return string.Format("{0:0000}-{1:00}-{2:00}", dt.Year, dt.Month, dt.Day);
                else
                    return string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}z",
                    dt.Year, dt.Month, dt.Day,
                    dt.Hour, dt.Minute, dt.Second);
            } 
            if(type == typeof(DateTimeOffset))
            {
                var dof = ((DateTimeOffset)value).ToUniversalTime();
                return string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}z", 
                    dof.Year, dof.Month, dof.Day,
                    dof.Hour, dof.Minute, dof.Second);
            }
            if (type == typeof(TimeSpan))
            {
                var ts = (TimeSpan)value;
                if (dateTimeType == QueryFilterCondition.IsDuration)
                    return string.Format("'P{0:0}DT{1:00}H{2:00}M{3:00}.{4:000}000000000}S'",
                        ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                else
                {
                    return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", 
                        ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                }

            }
            return null;
        }
        
    }
}
