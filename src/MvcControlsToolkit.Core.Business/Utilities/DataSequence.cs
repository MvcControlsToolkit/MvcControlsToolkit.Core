using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class DataSequence<T,S>
    {
        public ICollection<T> Data { get; set; }
        public S Continuation { get; set; }
    }
}
