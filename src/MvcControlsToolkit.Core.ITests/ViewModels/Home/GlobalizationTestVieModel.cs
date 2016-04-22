using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.ITests.ViewModels.Home
{
    public class GlobalizationTestVieModel
    {
        [Range(10.2, 20.2)]
        public float? AFloat { get; set; }
        [Range(typeof(DateTime), "2016-01-01T00:00:00", "2016-11-01T23:00:00")]
        public DateTime? ADatetime { get; set; }

        [Range(typeof(DateTime), "2016-01-01", "2016-11-01")]
        [DataType(DataType.Date)]
        public DateTime? ADate { get; set; }

        [DataType(DataType.Time)]
        [Range(typeof(TimeSpan), "10:00:00", "20:00:00")]
        public TimeSpan? ATime { get; set; }

        
        [Range(typeof(Week), "2016-W10", "2016-W30")]
        public Week? AWeek { get; set; }

        
        [Range(typeof(Month), "2016-01", "2016-10")]
        public Month? AMonth { get; set; }

        [DataType("Color")]
        public string AColor { get; set; }

        [DataType(DataType.Url)]
        public string AUrl { get; set; }

        public uint APositiveInteger { get; set; }
    }
}
