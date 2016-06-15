using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.DataAnnotations;

namespace WebTestCore.ViewModels
{
    public class DynamicRangeTestViewModel
    {
        
        [DynamicRange(typeof(float), SMaximum = "20", SMinimum = "10", Propagate =true )]
        [DynamicRange(typeof(float), DynamicMaximum = "AFloatMax+!1", DynamicMinimum = "AFloatMin-!1", Propagate = true)]
        public float? AFloat { get; set; }
        [DynamicRange(typeof(float), SMaximum = "20", SMinimum = "10", Propagate = true)]
        public float? AFloatMin { get; set; }
        [DynamicRange(typeof(float), SMaximum = "20", SMinimum = "10", Propagate = true)]
        public float? AFloatMax { get; set; }

        [DynamicRange(typeof(DateTime), DynamicMaximum = "ADatetimeMax+!1.00:00:00", DynamicMinimum = "ADatetimeMin-!1.00:00:00", Propagate = true)]
        public DateTime? ADatetime { get; set; }
        public DateTime? ADatetimeMin { get; set; }
        public DateTime? ADatetimeMax { get; set; }

        [DynamicRange(typeof(DateTime), DynamicMaximum = "ADateMax+!1.00:00:00", DynamicMinimum = "ADateMin-!1.00:00:00", Propagate = true)]
        [DataType(DataType.Date)]
        public DateTime? ADate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ADateMax { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ADateMin { get; set; }

        [DataType(DataType.Time)]
        
        [DynamicRange(typeof(TimeSpan), DynamicMaximum = "ATimeMax+!0.01:00:00", DynamicMinimum = "ATimeMin-!0.01:00:00", Propagate = true)]
        public TimeSpan? ATime { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan? ATimeMin { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan? ATimeMax { get; set; }

        
        [DynamicRange(typeof(Week), DynamicMaximum = "AWeekMax+!14.00:00:00", DynamicMinimum = "AWeekMin-!14.01:00:00", Propagate = true)]
        public Week? AWeek { get; set; }
        public Week? AWeekMin { get; set; }
        public Week? AWeekMax { get; set; }

        
        [DynamicRange(typeof(Month), DynamicMaximum = "AMonthMax+!31.00:00:00", DynamicMinimum = "AMonthMin-!31.01:00:00", Propagate = true)]
        public Month? AMonth { get; set; }
        public Month? AMonthMin { get; set; }
        public Month? AMonthMax { get; set; }

        [DataType("Color")]
        public string AColor { get; set; }

        [DataType(DataType.Url)]
        public string AUrl { get; set; }

        public uint APositiveInteger { get; set; }
    }
}
