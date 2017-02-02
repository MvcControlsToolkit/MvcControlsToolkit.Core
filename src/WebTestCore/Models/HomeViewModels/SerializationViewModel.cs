using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Types;

namespace WebTestCore.Models
{
    public class SerializationViewModel
    {
        public Month MonthTest { get; set; }
        public Week WeekTest { get; set; }

        public object SimpleTypeTest { get; set; }
    }
}
