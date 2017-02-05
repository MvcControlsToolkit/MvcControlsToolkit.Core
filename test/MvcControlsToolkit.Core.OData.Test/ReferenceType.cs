using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.OData.Test
{
    public class ReferenceType
    {
        public DateTime ADateTime { get; set; }
        public DateTime? ANDateTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime ADate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ANDate { get; set; }

        public Month AMonth { get; set; }
        public Month? ANMonth { get; set; }

        public Week AWeek { get; set; }
        public Week? ANWeek { get; set; }

        public DateTimeOffset ADateTimeOffset { get; set; }
        public DateTimeOffset? ANDateTimeOffset { get; set; }

        public TimeSpan ADuration { get; set; }
        public TimeSpan? ANDuration { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan ATime { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan? ANTime { get; set; }

        public double ADouble { get; set; }
        public double? ANDouble { get; set; }

        public float AFloat { get; set; }
        public float? ANFloat { get; set; }

        public decimal ADecimal { get; set; }
        public decimal? ANDecimal { get; set; }

        public long ALong { get; set; }
        public long? ANLong { get; set; }

        public int AInt { get; set; }
        public int? ANInt { get; set; }

        public short AShort { get; set; }
        public short? ANShort { get; set; }

        public string AString { get; set; }

        public bool ABool { get; set; }
        public bool? ANBool { get; set; }

        public Guid AGuid { get; set; }

        public Guid? ANGuid { get; set; }
    }
}
