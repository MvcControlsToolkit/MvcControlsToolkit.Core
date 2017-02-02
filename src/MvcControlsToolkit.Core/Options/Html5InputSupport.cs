using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Options.Attributes;

namespace MvcControlsToolkit.Core.Options
{
    public class Html5InputSupport
    {
        
        public static int Supported { get { return 3; } }
        public static int Simulated { get { return 2; } }
        public static int SimpleWidget { get { return 1; } }
        public static int NotSupported { get { return 0; } }
        
        public Html5InputSupport()
        {
            ClientTimeZoneOffset = System.DateTime.Now.Subtract(System.DateTime.UtcNow).TotalMinutes;
        }
        
        [OptionName("number")]
        public int Number { get; set; }
        [OptionName("range")]
        public int Range { get; set; }
        [OptionName("date")]
        public int Date { get; set; }
        [OptionName("month")]
        public int Month { get; set; }
        [OptionName("week")]
        public int Week { get; set; }
        [OptionName("time")]
        public int Time { get; set; }
        [OptionName("datetime")]
        public int DateTime { get; set; }
        [OptionName("email")]
        public int EMail { get; set; }
        [OptionName("search")]
        public int Search { get; set; }
        [OptionName("tel")]
        public int Tel { get; set; }
        [OptionName("url")]
        public int Url { get; set; }
        [OptionName("color")]
        public int Color { get; set; }
        [OptionName("clientTimeZoneOffset")]
        public double ClientTimeZoneOffset { get; set; }

        
    }
}
