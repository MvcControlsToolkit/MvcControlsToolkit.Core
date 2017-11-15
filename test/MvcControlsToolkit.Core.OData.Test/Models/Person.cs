using System;
using System.Collections.Generic;
using System.Text;
using MvcControlsToolkit.Core.DataAnnotations;

namespace MvcControlsToolkit.Core.OData.Test.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool Male { get; set; }
        [CollectionKey("Id")]
        public virtual ICollection<Person> Children { get; set; }
        public virtual Person Parent { get; set; }
        public virtual Person Spouse { get; set; }
        public virtual Person SpouseOf { get; set; }
        public  int? SpouseOfId { get; set; }
        public  int? ParentId { get; set; }
        
    }
}
