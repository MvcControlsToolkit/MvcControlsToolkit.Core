using System;
using System.Globalization;
using System.Linq.Expressions;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations.Queries;
using MvcControlsToolkit.Core.Types;

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
            if (operation.HasValue && (gres.Item2 == null || !gres.Item2.Allowed(operation.Value)))
                throw new OperationNotAllowedException(name, operationName);
            foreach (var property in gres.Item1)
            {
                curr = Expression.Property(curr, property);
            }
            return curr;
        }
        internal Expression BuildCall(string name, Expression par, Type t, Expression arg)
        {
            var method = QueryNodeCache.GetMethod(t, name, arg==null ? null : arg.Type);
            return arg == null ? Expression.Call(method, par) : Expression.Call(method, par, arg);
        }
        internal string EncodeProperty(string name)
        {
            if (name == null) return null;
            return name.Replace('.', '/');
        }
        internal string encodeConstant(object value, short dateTimeType)
        {
            if (value == null) return "null";
            if (value is Enum) value = Convert.ChangeType(value,
                Type.GetType("System." + Enum.GetName(typeof(TypeCode), Convert.GetTypeCode(value))));
            var type = value.GetType();
            type = Nullable.GetUnderlyingType(type) ?? type;
            if(type == typeof(Month))
            {
                type = typeof(DateTime);
                value = ((Month)value).ToDateTime();
            }
            else if(type == typeof(Week))
            {
                type = typeof(DateTime);
                value = ((Week)value).StartDate();
            }
            if (type == typeof(string)) return "'"+(value as string).Replace("'", "''") + "'";
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
            if (type == typeof(string)) return "'" + value.ToString().Replace("'", "''") + "'";
            if (type == typeof(Guid)) return value.ToString();
            if (type == typeof(bool)) return ((bool)value) ? "true" : "false";
            if (type == typeof(DateTime))
            {
                var dt = (DateTime)value;
                if(dateTimeType == QueryFilterCondition.IsDate)
                    return string.Format("{0:0000}-{1:00}-{2:00}", dt.Year, dt.Month, dt.Day);
                else
                {
                    if (dt.Kind == DateTimeKind.Local) dt = dt.ToUniversalTime();
                    return dt.Millisecond>0 ?
                        string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}.{6:000}Z",
                    dt.Year, dt.Month, dt.Day,
                    dt.Hour, dt.Minute, dt.Second, dt.Millisecond) :
                        string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}Z",
                    dt.Year, dt.Month, dt.Day,
                    dt.Hour, dt.Minute, dt.Second);
                }
                    
            } 
            if(type == typeof(DateTimeOffset))
            {
                var dof = ((DateTimeOffset)value).ToUniversalTime();
                return 
                    dof.Millisecond>0 ?
                    string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}.{6:000}Z",
                    dof.Year, dof.Month, dof.Day,
                    dof.Hour, dof.Minute, dof.Second):
                    string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}Z", 
                    dof.Year, dof.Month, dof.Day,
                    dof.Hour, dof.Minute, dof.Second);
            }
            if (type == typeof(TimeSpan))
            {
                var ts = (TimeSpan)value;
                if (dateTimeType == QueryFilterCondition.IsDuration)
                    return string.Format("duration'P{0:0}DT{1:00}H{2:00}M{3:00}.{4:000}000000000S'",
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
