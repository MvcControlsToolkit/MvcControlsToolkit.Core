using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.OData.Test.Models
{
    public class ReferenceModel
    {
        public int Id { get; set; }
        public DateTime ADateTime { get; set; }
        public DateTime? ANDateTime { get; set; }

       
        public DateTime ADate { get; set; }
        
        public DateTime? ANDate { get; set; }

        public DateTime AMonth { get; set; }
        public DateTime? ANMonth { get; set; }

        public DateTime AWeek { get; set; }
        public DateTime? ANWeek { get; set; }

        public DateTimeOffset ADateTimeOffset { get; set; }
        public DateTimeOffset? ANDateTimeOffset { get; set; }

        public TimeSpan ADuration { get; set; }
        public TimeSpan? ANDuration { get; set; }

        
        public TimeSpan ATime { get; set; }
        
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
