using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.TagHelpers;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public class ReductionContext
    {
        public IList<ReductionResult> Results {get; private set;}
        public int CurrentToken { get; private set; }
        public int CurrentSubtoken { get; private set; }
        public DefaultTemplates Defaults { get; private set; }

        public bool RowParsingDisabled { get; private set; }
        public ReductionContext(int token, int subToken, DefaultTemplates defaults, bool rowParsingDisabled=false)
        {
            Results = new List<ReductionResult>();
            CurrentToken = token;
            CurrentSubtoken = subToken;
            Defaults = defaults;
            RowParsingDisabled = rowParsingDisabled;
        }
    }
}
