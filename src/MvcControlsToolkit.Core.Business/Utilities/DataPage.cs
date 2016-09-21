using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.Business.Utilities
{
    public class DataPage<K>
    {
        public ICollection<K> Data { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
