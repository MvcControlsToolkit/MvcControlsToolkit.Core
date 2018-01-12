using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Transformations.Internals;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.OData;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.Business.Transformations
{
    public class ODataTransformationRepository: TransformationRepository, IWebQueryable
    {
        public ODataTransformationRepository(ICRUDRepository repository, MappingContext context, IDictionary<Type, TransformationRepositoryInternal> allowedMappings)
            : base(repository, context, allowedMappings)
        {

        }

        public async Task<DataPage<D>> ExecuteQuery<D, Dext>(IWebQueryProvider query) where Dext : D
        {
            var prep = findMap<D>();
            if(prep==null)
            {
                if (repository is IWebQueryable)
                    return await (repository as IWebQueryable).ExecuteQuery<D, Dext>(query);
                else
                {
                    QueryDescription<D> qd = null;
                    if (query != null)
                    {
                        qd = query.Parse<D>();
                    }

                    if (qd == null) return await
                            repository.GetPage<D>(null, null, 1, int.MaxValue, null);
                    else
                    {
                        var grouping = qd.GetGrouping<Dext>();
                        if (grouping == null)
                        {
                            return await
                                repository.GetPage<D>(qd.GetFilterExpression(), qd.GetSorting(), (int)qd.Page, (int)qd.Take);
                        }
                        else
                        {
                            return await
                                repository.GetPageExtended<D, Dext>(qd.GetFilterExpression(), qd.GetSorting<Dext>(), (int)qd.Page, (int)qd.Take, grouping);
                        }
                    }
                }
                
            }
            return await prep.ExecuteQuery<D, Dext>(repository, context, query);
        }
    }
}
