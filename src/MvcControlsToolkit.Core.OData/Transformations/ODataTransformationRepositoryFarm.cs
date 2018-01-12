using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.Business.Transformations.Internals;

namespace MvcControlsToolkit.Core.Business.Transformations
{
    public class ODataTransformationRepositoryFarm
    {
        protected Dictionary<Type, TransformationRepositoryInternal> allowedMappings = new Dictionary<Type, TransformationRepositoryInternal>();
        public virtual ODataTransformationRepository Create(ICRUDRepository repository, MappingContext context = null)
        {
            return new ODataTransformationRepository(repository, context, allowedMappings);
        }
        public virtual ODataTransformationRepositoryFarm Add<VM, DTO>()
        {
            allowedMappings[typeof(VM)] = new TransformationRepositoryInternal<DTO>();
            return this;
        }

        public virtual ODataTransformationRepositoryFarm Add<VM, DTO, DTOext>()
            where DTOext: DTO
        {
            allowedMappings[typeof(VM)] = new TransformationRepositoryInternal<DTO, DTOext>();
            return this;
        }
    }
}
