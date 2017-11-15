using System;
using System.Collections.Generic;
using System.Text;
using MvcControlsToolkit.Core.DataAnnotations;

namespace MvcControlsToolkit.Core.OData.Test
{
    public class PersonDTO
    {
        public int? Id { get; set; }
        [Query]
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool Male { get; set; }
        public virtual IEnumerable<PersonDTO> Children { get; set; }
        public virtual PersonDTO Spouse { get; set; }

    }
    public class PersonDTOAuto: PersonDTO
    {

    }
}
