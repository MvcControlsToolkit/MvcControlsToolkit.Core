using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.DataAnnotations;

namespace WebTestCore.Models
{
    public class Person
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
    [RunTimeType]
    public class Customer:Person
    {
        public string RegisterNumber { get; set; }
        
    }
    [RunTimeType]
    public class Employee : Person
    {
        public string Matr { get; set; }

    }
}
