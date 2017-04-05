using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTestCore.Models
{
    public class TestInterfaceViewModel: ITestInterface
    {
        public int TestProperty { get; set; }
    }
}
