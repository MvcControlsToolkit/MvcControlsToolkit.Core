using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvcControlsToolkit.Core.Filters
{
    public class CacheViewPartsFilter : IResultFilter
    {
        private const string entry = "_AfterResultActions_";
        public static void AddAction(HttpContext ctx, Action action)
        {
            object res;
            List<Action> fres;
            if (ctx.Items.TryGetValue(entry, out res) && res is List<Action>)
            {
                fres = res as List<Action>;
            }
            else
            {
                fres = new List<Action>();
                ctx.Items.Add(entry, fres);
            }
            fres.Add(action);
        }
        public void OnResultExecuted(ResultExecutedContext context)
        {
            object res;
            if(context.HttpContext.Items.TryGetValue(entry, out res))
            {
                List<Action> fres = res as List<Action>;
                if (fres == null) return;
                foreach (Action action in fres) action();
            }
            
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            
        }
    }
}
