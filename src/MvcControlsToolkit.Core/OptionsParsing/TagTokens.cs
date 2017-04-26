using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public static class TagTokens
    {
        public static int RowContainer {get { return 1; } }
        public static int Row { get { return 2; } }
        public static int Column { get { return 3; } }
        public static int DTemplate { get { return 4; } }
        public static int ETemplate { get { return 5; } }
        public static int FTemplate { get { return 8; } }
        public static int STemplate { get { return 9; } }
        public static int GTemplate { get { return 10; } }
        public static int Content { get { return 6; } }

        public static int ExternalKeyConnection { get { return 7; } }
       
    }
}
