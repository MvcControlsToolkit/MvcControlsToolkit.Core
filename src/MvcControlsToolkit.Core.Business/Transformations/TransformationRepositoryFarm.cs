using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Transformations.Internals;
using MvcControlsToolkit.Core.Business.Utilities;

namespace MvcControlsToolkit.Core.Business.Transformations
{
    public class TransformationRepositoryFarm
    {
        protected Dictionary<Type, TransformationRepositoryInternal> allowedMappings= new Dictionary<Type, TransformationRepositoryInternal>();
        public virtual TransformationRepository Create(ICRUDRepository repository, MappingContext context=null)
        {
            return new TransformationRepository(repository, context, allowedMappings);
        }
        public virtual TransformationRepositoryFarm Add<VM, DTO>()
        {
            allowedMappings[typeof(VM)] = new TransformationRepositoryInternal<DTO>();
            return this;
        }
    }
}
