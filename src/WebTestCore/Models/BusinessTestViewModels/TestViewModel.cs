using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTestCore.Models
{
    public interface ITestViewModel
    {
        int? Id { get; set; }

        string FieldA { get; set; }

        string FieldB { get; set; }

        string FieldBC { get; set; }

        string FieldD { get; set; }

        string FieldE { get; set; }

        
    }
    public class TestViewModel: ITestViewModel
    {
        public int? Id { get; set; }
        
        public string FieldA { get; set; }
        
        public string FieldB { get; set; }

        public string FieldBC { get; set; }

        public string FieldD { get; set; }

        public string FieldE { get; set; }

        public string FieldF { get; set; }
    }
}
