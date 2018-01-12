using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.OData;
using MvcControlsToolkit.Core.Views;
using MvcControlsToolkit.Core.Business.Transformations;

namespace MvcControlsToolkit.Core.Business.Transformations.Internals
{
    public class TransformationRepositoryInternal<DTO, DTOEXT>
        : TransformationRepositoryInternal<DTO> where DTOEXT : DTO
    {
        public async override Task<DataPage<D>> ExecuteQuery<D, D1>(ICRUDRepository repo, MappingContext context, object query)
        {
            var cQuery = query as IWebQueryProvider;
            DataPage<DTO> pRes = null;
            bool hasGrouping = false;
            if (repo is IWebQueryable)
            {
                
                pRes = await (repo as IWebQueryable).ExecuteQuery<DTO, DTOEXT>(cQuery);
                hasGrouping = pRes != null && pRes.Data != null && pRes.Data.Count > 0 && pRes.Data.First() is DTOEXT;
            }
            else
            {
                QueryDescription<DTO> qd = null;
                if (cQuery != null)
                {
                    qd = cQuery.Parse<DTO>();
                }

                if (qd == null) pRes = await
                        repo.GetPage<DTO>(null, null, 1, int.MaxValue, null);
                else
                {
                    var grouping = qd.GetGrouping<DTOEXT>();
                    if (grouping == null)
                    {
                        pRes = await
                        repo.GetPage<DTO>(qd.GetFilterExpression(), qd.GetSorting(), (int)qd.Page, (int)qd.Take);
                    }
                    else
                    {
                        pRes = await
                        repo.GetPageExtended<DTO, DTOEXT>(qd.GetFilterExpression(), qd.GetSorting<DTOEXT>(), (int)qd.Page, (int)qd.Take, grouping);
                        hasGrouping = true;
                    }
                }
            }
            if (pRes == null) return null;
            var res = new DataPage<D>()
            {
                ItemsPerPage = pRes.ItemsPerPage,
                Page = pRes.Page,
                TotalCount = pRes.TotalCount,
                TotalPages = pRes.TotalPages
            };
            if (hasGrouping)
                res.Data = pRes.Data.Select(m => m == null ? default(DTOEXT) : (DTOEXT)m).MapIEnumerable<DTOEXT>(context).To<D1>().Select(m => m == null ? default(D) : (D)m).ToList();
            else
                res.Data = pRes.Data.MapIEnumerable<DTO>(context).To<D>().ToList();
            return res;
        }
    }
}
