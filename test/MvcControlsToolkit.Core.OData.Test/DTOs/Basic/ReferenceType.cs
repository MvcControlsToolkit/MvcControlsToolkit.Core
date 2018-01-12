using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.OData.Test
{
    public enum TestEnum { value1, value2, value3 }

    public class ReferenceType
    {
        [Query]
        public int? Id { get; set; }
        [Query]
        public DateTime ADateTime { get; set; }
        [Query]
        public DateTime? ANDateTime { get; set; }
        [Query]
        [DataType(DataType.Date)]
        public DateTime ADate { get; set; }
        [Query]
        [DataType(DataType.Date)]
        public DateTime? ANDate { get; set; }
        [Query]
        public Month AMonth { get; set; }
        [Query]
        public Month? ANMonth { get; set; }
        [Query]
        public Week AWeek { get; set; }
        [Query]
        public Week? ANWeek { get; set; }
        [Query]
        public DateTimeOffset ADateTimeOffset { get; set; }
        [Query]
        public DateTimeOffset? ANDateTimeOffset { get; set; }
        [Query]

        public TimeSpan ADuration { get; set; }
        [Query]
        public TimeSpan? ANDuration { get; set; }
        [Query]
        [DataType(DataType.Time)]
        public TimeSpan ATime { get; set; }
        [Query]
        [DataType(DataType.Time)]
        public TimeSpan? ANTime { get; set; }
        [Query]
        public double ADouble { get; set; }
        [Query]
        public double? ANDouble { get; set; }
        [Query]
        public float AFloat { get; set; }
        [Query]
        public float? ANFloat { get; set; }
        [Query]
        public decimal ADecimal { get; set; }
        [Query]
        public decimal? ANDecimal { get; set; }
        [Query]
        public long ALong { get; set; }
        [Query]
        public long? ANLong { get; set; }
        [Query]
        public int AInt { get; set; }
        [Query]
        public int? ANInt { get; set; }
        [Query]
        public short AShort { get; set; }
        [Query]
        public short? ANShort { get; set; }
        [Query(Deny = QueryOptions.None)]
        public string AString { get; set; }
        [Query]
        public bool ABool { get; set; }
        [Query]
        public bool? ANBool { get; set; }
        [Query]
        public Guid AGuid { get; set; }
        [Query]
        public Guid? ANGuid { get; set; }

        public TestEnum AnEnum { get; set; }
    }
    public class ReferenceTypeExtended: ReferenceType
    {
        public int ANewInt { get; set; }
    }

    public class ReferenceTypeWithChildren: ReferenceType, IFilterReferenceTypeWithChildren
    {
        public IEnumerable<IFilterNestedReferenceType> Children { get; set; }
    }
    public interface IFilterReferenceTypeWithChildren
    {
        [Query]
        int? Id { get; set; }
        [Query]
         bool ABool { get; set; }
        [Query]
        Month AMonth { get; set; }
        [Query]
        Month? ANMonth { get; set; }
        [Query]
        Week AWeek { get; set; }
        [Query]
        Week? ANWeek { get; set; }
        [Query(Deny = QueryOptions.None)]
        string AString { get; set; }

        

        IEnumerable<IFilterNestedReferenceType> Children { get; set; }
    }

    public class ReferenceVM
    {
        [Query]
        public int? Id { get; set; }
        [Query]
        public DateTime ADateTime { get; set; }
        [Query]
        public DateTime? ANDateTime { get; set; }
        [Query]
        [DataType(DataType.Date)]
        public DateTime ADate { get; set; }
        [Query]
        [DataType(DataType.Date)]
        public DateTime? ANDate { get; set; }
        [Query]
        public Month AMonth { get; set; }
        [Query]
        public Month? ANMonth { get; set; }
        [Query]
        public Week AWeek { get; set; }
        [Query]
        public Week? ANWeek { get; set; }
        [Query]
        public DateTimeOffset ADateTimeOffset { get; set; }
        [Query]
        public DateTimeOffset? ANDateTimeOffset { get; set; }
        [Query]

        public TimeSpan ADuration { get; set; }
        [Query]
        public TimeSpan? ANDuration { get; set; }
        [Query]
        [DataType(DataType.Time)]
        public TimeSpan ATime { get; set; }
        [Query]
        [DataType(DataType.Time)]
        public TimeSpan? ANTime { get; set; }
        [Query]
        public double ADouble { get; set; }
        [Query]
        public double? ANDouble { get; set; }
        [Query]
        public float AFloat { get; set; }
        [Query]
        public float? ANFloat { get; set; }
        [Query]
        public decimal ADecimal { get; set; }
        [Query]
        public decimal? ANDecimal { get; set; }
        [Query]
        public long ALong { get; set; }
        [Query]
        public long? ANLong { get; set; }
        [Query]
        public int AInt { get; set; }
        [Query]
        public int? ANInt { get; set; }
        [Query]
        public short AShort { get; set; }
        [Query]
        public short? ANShort { get; set; }
        [Query(Deny = QueryOptions.None)]
        public string AString { get; set; }
        [Query]
        public bool ABool { get; set; }
        [Query]
        public bool? ANBool { get; set; }
        [Query]
        public Guid AGuid { get; set; }
        [Query]
        public Guid? ANGuid { get; set; }

        public TestEnum AnEnum { get; set; }
    }
    public class ReferenceVMExtended : ReferenceVM
    {
        public int ANewInt { get; set; }
    }
}
