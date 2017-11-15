using System;
using System.Collections.Generic;
using System.Text;
using MvcControlsToolkit.Core.Business;
using MvcControlsToolkit.Core.DataAnnotations;

namespace MvcControlsToolkit.Core.OData.Test
{
    public class PersonDTOFlattened
    {
        public int? Id { get; set; }
        [Query]
        public string Name { get; set; }
        public string Surname { get; set; }
        public IEnumerable<PersonDTOFlattened> Children { get; set; }
        public string SpouseName { get; set; }
        public string SpouseSurname { get; set; }
        [CollectionChange(CollectionChangeMode.Add)]
        public IEnumerable<PersonDTOFlattened> SpouseChildren { get; set; }
    }
    
    public class PersonDTOFlattenedAuto : PersonDTOFlattened, IUpdateConnections
    {
        public bool MayUpdate(string prefix)
        {
            return prefix == "Spouse";
        }
    }

}
