using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.DataAnnotations;
using MvcControlsToolkit.Core.Types;

namespace WebTestFramework.ViewModels
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
        [DynamicRange(typeof(TimeSpan), SMaximum = "20:00:00", SMinimum = "10:00:00")]
        public TimeSpan? ATime { get; set; }

        [DynamicRange(typeof(Week), SMaximum = "2016-W30", SMinimum = "2016-W10")]
        public Week? AWeek { get; set; }

        
        
        [DynamicRange(typeof(Month), SMaximum = "2016-10", SMinimum = "2016-01")]
        public Month? AMonth { get; set; }

        [DataType("Color")]
        public string AColor { get; set; }

        [DataType(DataType.Url)]
        public string AUrl { get; set; }

        public uint APositiveInteger { get; set; }
    }
}
