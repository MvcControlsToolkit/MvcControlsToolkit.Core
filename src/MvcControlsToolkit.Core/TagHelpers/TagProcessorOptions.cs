using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public class TagProcessorOptions
    {
        public IList<RowType> Rows { get; private set; }
        public TagProcessorOptions(IList<RowType> rows)
        {
            Rows = rows;
        }
    }
}
