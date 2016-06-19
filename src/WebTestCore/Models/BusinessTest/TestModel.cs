using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebTestCore.Models
{
    public class TestModel
    {
        public int Id { get; set; }
        [MaxLength(64)]
        public string FieldA { get; set; }
        [MaxLength(64)]
        public string FieldB { get; set; }
        [MaxLength(64)]
        public string FieldC { get; set; }
        [MaxLength(64)]
        public string FieldD { get; set; }
        [MaxLength(64)]
        public string FieldE { get; set; }
        [MaxLength(64)]
        public string FieldF { get; set; }
    }
}
