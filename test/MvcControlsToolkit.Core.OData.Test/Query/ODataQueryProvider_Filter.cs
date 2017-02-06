using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;
using Xunit;
using Xunit.Abstractions;

namespace MvcControlsToolkit.Core.OData.Test.Query
{
    public class ODataQueryProvider_Filter
    {
        ODataQueryProvider provider;
        private readonly ITestOutputHelper output;
        public ODataQueryProvider_Filter(ITestOutputHelper output)
        {
            provider = new ODataQueryProvider();
            this.output = output;
        }
        [Theory]
        [InlineData("ABool eq true", typeof(bool), "ABool", true)]
        [InlineData("ANBool eq false", typeof(bool), "ANBool", false)]
        [InlineData("ALong eq 1", typeof(long), "ALong", (long)1)]
        [InlineData("ANLong eq 2", typeof(long), "ANLong", (long)2)]
        [InlineData("AInt eq 1", typeof(int), "AInt", 1)]
        [InlineData("ANInt eq 2", typeof(int), "ANInt", 2)]
        [InlineData("AShort eq 1", typeof(short), "AShort", (short)1)]
        [InlineData("ANShort eq 2", typeof(short), "ANShort", (short)2)]
        [InlineData("ADouble eq 1.5", typeof(double), "ADouble", 1.5)]
        [InlineData("ANDouble eq 2.5", typeof(double), "ANDouble", 2.5)]
        [InlineData("AFloat eq 1.5", typeof(float), "AFloat", 1.5f)]
        [InlineData("ANFloat eq 2.5", typeof(float), "ANFloat", 2.5f)]
        [InlineData("AString eq 'Hello'", typeof(string), "AString", "Hello")]
        [InlineData("ADecimal eq 1.5", typeof(decimal), "ADecimal", "1.5")]
        [InlineData("ANDecimal eq 2.5", typeof(decimal), "ANDecimal", "2.5")]
        [InlineData("AGuid eq 01234567-89ab-cdef-0123-456789abcdef", typeof(Guid), "AGuid", "01234567-89ab-cdef-0123-456789abcdef")]
        [InlineData("ANGuid eq 01234567-89ab-cdef-0123-456789abcdef", typeof(Guid), "ANGuid", "01234567-89ab-cdef-0123-456789abcdef")]
        [InlineData("ATime eq 07:59:59.999", typeof(TimeSpan), "ATime", "07:59:59.999")]
        [InlineData("ANTime eq 07:59:59.999", typeof(TimeSpan), "ANTime", "07:59:59.999")]
        [InlineData("ADuration eq duration'P12DT23H59M59.999999999999S'", typeof(TimeSpan), "ADuration", "12:23:59:59.9999999")]
        [InlineData("ANDuration eq duration'P12DT23H59M59.999999999999S'", typeof(TimeSpan), "ANDuration", "12:23:59:59.9999999")]
        [InlineData("ADate eq 2012-12-03", typeof(DateTime), "ADate", "2012-12-03")]
        [InlineData("ANDate eq 2012-12-03", typeof(DateTime), "ANDate", "2012-12-03")]
        [InlineData("AMonth eq 2012-11-01", typeof(Month), "AMonth", "2012-11")]
        [InlineData("ANMonth eq 2012-11-01", typeof(Month), "ANMonth", "2012-11")]
        [InlineData("AWeek eq 2012-11-01", typeof(Week), "AWeek", "2012-W44")]
        [InlineData("ANWeek eq 2012-11-01", typeof(Week), "ANWeek", "2012-W44")]
        [InlineData("ADateTime eq 2012-12-03T07:16:23Z", typeof(DateTime), "ADateTime", "2012-12-03T07:16:23")]
        [InlineData("ANDateTime eq 2012-12-03T07:16:23Z", typeof(DateTime), "ANDateTime", "2012-12-03T07:16:23")]
        [InlineData("ADateTimeOffset eq 2012-12-03T07:16:23Z", typeof(DateTimeOffset), "ADateTimeOffset", "2012-12-03T07:16:23Z")]
        [InlineData("ANDateTimeOffset eq 2012-12-03T07:16:23Z", typeof(DateTimeOffset), "ANDateTimeOffset", "2012-12-03T07:16:23Z")]
        public void ConstantsEncoding(string filter, Type valueType, string propertyName, object value)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);

            Assert.NotNull(res.Filter);
            Assert.Equal(res.Filter.Operator, QueryFilterBooleanOperator.and);
            Assert.Null(res.Filter.Child1);
            Assert.Null(res.Filter.Child2);
            Assert.Null(res.Filter.Argument2);
            var cond = res.Filter.Argument1;
            Assert.NotNull(cond);
            Assert.Equal(cond.Operator, "eq");
            Assert.Equal(cond.Property, propertyName);
            Assert.NotNull(cond.Value);
            Assert.IsType(valueType, cond.Value);

            if (value != null)
            {
                if (value is string && valueType != typeof(string))
                {
                    var conv = TypeDescriptor.GetConverter(valueType);
                    value = conv.ConvertFrom(
                        context: null,
                        culture: CultureInfo.InvariantCulture,
                        value: value);
                }

                Assert.Equal(cond.Value, value);
            }
        }
        [Theory]
        [InlineData("ABool eq true", typeof(bool), "ABool", true)]
        [InlineData("ANBool eq false", typeof(bool), "ANBool", false)]
        [InlineData("ALong eq 1", typeof(long), "ALong", (long)1)]
        [InlineData("ANLong eq 2", typeof(long), "ANLong", (long)2)]
        [InlineData("AInt eq 1", typeof(int), "AInt", 1)]
        [InlineData("ANInt eq 2", typeof(int), "ANInt", 2)]
        [InlineData("AShort eq 1", typeof(short), "AShort", (short)1)]
        [InlineData("ANShort eq 2", typeof(short), "ANShort", (short)2)]
        [InlineData("ADouble eq 1.5", typeof(double), "ADouble", 1.5)]
        [InlineData("ANDouble eq 2.5", typeof(double), "ANDouble", 2.5)]
        [InlineData("AFloat eq 1.5", typeof(float), "AFloat", 1.5f)]
        [InlineData("ANFloat eq 2.5", typeof(float), "ANFloat", 2.5f)]
        [InlineData("AString eq 'Hello'", typeof(string), "AString", "Hello")]
        [InlineData("ADecimal eq 1.5", typeof(decimal), "ADecimal", "1.5")]
        [InlineData("ANDecimal eq 2.5", typeof(decimal), "ANDecimal", "2.5")]
        [InlineData("AGuid eq 01234567-89ab-cdef-0123-456789abcdef", typeof(Guid), "AGuid", "01234567-89ab-cdef-0123-456789abcdef")]
        [InlineData("ANGuid eq 01234567-89ab-cdef-0123-456789abcdef", typeof(Guid), "ANGuid", "01234567-89ab-cdef-0123-456789abcdef")]
        [InlineData("ATime eq 07:59:59.999", typeof(TimeSpan), "ATime", "07:59:59.999")]
        [InlineData("ANTime eq 07:59:59.999", typeof(TimeSpan), "ANTime", "07:59:59.999")]
        [InlineData("ADuration eq duration'P12DT23H59M59.999999999999S'", typeof(TimeSpan), "ADuration", "12:23:59:59.9999999")]
        [InlineData("ANDuration eq duration'P12DT23H59M59.999999999999S'", typeof(TimeSpan), "ANDuration", "12:23:59:59.9999999")]
        [InlineData("ADate eq 2012-12-03", typeof(DateTime), "ADate", "2012-12-03")]
        [InlineData("ANDate eq 2012-12-03", typeof(DateTime), "ANDate", "2012-12-03")]
        [InlineData("AMonth eq 2012-11-01", typeof(Month), "AMonth", "2012-11")]
        [InlineData("ANMonth eq 2012-11-01", typeof(Month), "ANMonth", "2012-11")]
        [InlineData("AWeek eq 2012-11-01", typeof(Week), "AWeek", "2012-W44")]
        [InlineData("ANWeek eq 2012-11-01", typeof(Week), "ANWeek", "2012-W44")]
        [InlineData("ADateTime eq 2012-12-03T07:16:23Z", typeof(DateTime), "ADateTime", "2012-12-03T07:16:23")]
        [InlineData("ANDateTime eq 2012-12-03T07:16:23Z", typeof(DateTime), "ANDateTime", "2012-12-03T07:16:23")]
        [InlineData("ADateTimeOffset eq 2012-12-03T07:16:23Z", typeof(DateTimeOffset), "ADateTimeOffset", "2012-12-03T07:16:23Z")]
        [InlineData("ANDateTimeOffset eq 2012-12-03T07:16:23Z", typeof(DateTimeOffset), "ANDateTimeOffset", "2012-12-03T07:16:23Z")]
        public void ConstantsToString(string filter, Type valueType, string propertyName, object value)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();
            filter = res.Filter.ToString();
            filter = filter.Substring(8);
            ConstantsEncoding(filter, valueType, propertyName, value);
        }
        [Theory]
        [InlineData("AString ne 'Hello'", "ne", "AString", false)]
        [InlineData("AString eq 'Hello'", "eq", "AString", false)]
        [InlineData("AString gt 'Hello'", "gt", "AString", false)]
        [InlineData("AString ge 'Hello'", "ge", "AString", false)]
        [InlineData("AString lt 'Hello'", "lt", "AString", false)]
        [InlineData("AString le 'Hello'", "le", "AString", false)]
        [InlineData("'Hello' ne AString ", "ne", "AString", false)]
        [InlineData("'Hello' eq AString ", "eq", "AString", false)]
        [InlineData("'Hello' gt AString ", "le", "AString", false)]
        [InlineData("'Hello' ge AString ", "lt", "AString", false)]
        [InlineData("'Hello' lt AString ", "ge", "AString", false)]
        [InlineData("'Hello' le AString ", "gt", "AString", false)]
        [InlineData("contains(AString,'Hello')", "contains", "AString", false)]
        [InlineData("startswith(AString,'Hello')", "startswith", "AString", false)]
        [InlineData("endswith(AString,'Hello')", "endswith", "AString", false)]
        [InlineData("contains('Hello', AString)", "contains", "AString", true)]
        [InlineData("startswith('Hello', AString)", "startswith", "AString", true)]
        [InlineData("endswith('Hello', AString)", "endswith", "AString", true)]
        public void OperatorsEncoding(string filter, string op, string propertyName, bool inv)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);

            Assert.NotNull(res.Filter);
            Assert.Equal(res.Filter.Operator, QueryFilterBooleanOperator.and);
            Assert.Null(res.Filter.Child1);
            Assert.Null(res.Filter.Child2);
            Assert.Null(res.Filter.Argument2);
            var cond = res.Filter.Argument1;
            Assert.NotNull(cond);
            Assert.Equal(cond.Operator, op);
            Assert.Equal(cond.Inv, inv);
            Assert.Equal(cond.Property, propertyName);
            Assert.NotNull(cond.Value);

        }
        [Theory]
        [InlineData("AString ne 'Hello'", "ne", "AString", false)]
        [InlineData("AString eq 'Hello'", "eq", "AString", false)]
        [InlineData("AString gt 'Hello'", "gt", "AString", false)]
        [InlineData("AString ge 'Hello'", "ge", "AString", false)]
        [InlineData("AString lt 'Hello'", "lt", "AString", false)]
        [InlineData("AString le 'Hello'", "le", "AString", false)]
        [InlineData("'Hello' ne AString ", "ne", "AString", false)]
        [InlineData("'Hello' eq AString ", "eq", "AString", false)]
        [InlineData("'Hello' gt AString ", "le", "AString", false)]
        [InlineData("'Hello' ge AString ", "lt", "AString", false)]
        [InlineData("'Hello' lt AString ", "ge", "AString", false)]
        [InlineData("'Hello' le AString ", "gt", "AString", false)]
        [InlineData("contains(AString,'Hello')", "contains", "AString", false)]
        [InlineData("startswith(AString,'Hello')", "startswith", "AString", false)]
        [InlineData("endswith(AString,'Hello')", "endswith", "AString", false)]
        [InlineData("contains('Hello', AString)", "contains", "AString", true)]
        [InlineData("startswith('Hello', AString)", "startswith", "AString", true)]
        [InlineData("endswith('Hello', AString)", "endswith", "AString", true)]
        public void OperatorsToString(string filter, string op, string propertyName, bool inv)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();
            filter = res.Filter.ToString();
            filter = filter.Substring(8);
            OperatorsEncoding(filter, op, propertyName, inv);
        }
        [Theory]
        [InlineData("AString ne 'Hello' and ADouble eq 1.5", QueryFilterBooleanOperator.and)]
        [InlineData("AString ne 'Hello' or ADouble eq 1.5", QueryFilterBooleanOperator.or)]
        [InlineData("AString ne 'Hello' and not (ADouble eq 1.5)", QueryFilterBooleanOperator.and)]
        [InlineData("not (ADouble eq 1.5)", QueryFilterBooleanOperator.not)]
        public void LogicalOperators(string filter, int op)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();
            Assert.NotNull(res);
            Assert.NotNull(res.Filter);
            Assert.Equal(res.Filter.Operator, op);
            filter = res.Filter.ToString();
            Assert.NotNull(filter);
            Assert.NotEqual(filter.Trim(), string.Empty);
            provider.Filter = filter.Substring(8);
            res = provider.Parse<ReferenceType>();
            Assert.Equal(filter, res.Filter.ToString());
        }
    }
}
