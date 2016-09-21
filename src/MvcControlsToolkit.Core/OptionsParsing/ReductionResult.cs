using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.OptionsParsing
{
    public class ReductionResult
    {
        public int Token { get; private set; }
        public int SubToken { get; private set; }
        public object Result { get; private set; }

        public ReductionResult(int token, int subToken, object result)
        {
            Token = token;
            SubToken = subToken;
            Result = result;
        }
    }
}
