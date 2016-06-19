using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebTestCore.Data;
using MvcControlsToolkit.Core.Linq;
using MvcControlsToolkit.Core.Business.Utilities;
using Microsoft.EntityFrameworkCore;

namespace WebTestCore.Controllers
{
    public class BusinessTestController : Controller
    {
        ApplicationDbContext ctx;
        public BusinessTestController(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }
        public async Task<IActionResult> Index()
        {
            //clear database
            ctx.Database.ExecuteSqlCommand("delete  from dbo.TestModels");
            //populate db
            var population = new List<Models.TestModel>();
            for (int i = 0; i < 10; i++)
            {
                var curr = new Models.TestModel
                {
                    FieldA = "FieldA_" + i,
                    FieldB = "FieldB_" + i,
                    FieldC = "FieldC_" + i,
                    FieldD = "FieldD_" + i,
                    FieldE = "FieldE_" + i,
                    FieldF = "FieldF_" + i

                };
                population.Add(curr);
                ctx.TestModels.Add(curr);
            }
            Expression<Func<Models.TestModel, bool>> test = m => (new int[] { 10, 3 }).Contains(m.Id);  
            await ctx.SaveChangesAsync();
            //reset context...we are going into a different web page
            foreach (var item in population) ctx.Entry(item).State = EntityState.Detached;

            //
            //get data
            var original = await ctx.TestModels.Project().To<Models.TestViewModel>().ToArrayAsync();
            //

            //perform changes (this usually happens in user interface)...
            var copier = new ObjectCopier<Models.TestViewModel, Models.TestViewModel>();
            var changed = original.Select(m => copier.Copy(m, new Models.TestViewModel())).ToList();

            changed[3].FieldD = "FieldD3_Changed";
            changed[5].FieldF = "FieldF%_Changed";
            changed.Remove(changed[7]);
            changed.Add(new Models.TestViewModel
            {
                FieldA = "FieldA_New",
                FieldB = "FieldB_Bew",
                FieldD = "FieldD_New",
                FieldE = "FieldE_New",
                FieldF = "FieldF_New"

            });
            //
            //compute change set
            var changes = ChangeSet.Create(original, changed, m => m.Id);
            //
            //pass changes to db
            try
            {
                await changes.UpdateDatabase(ctx.TestModels, ctx, saveChanges: true);
            }
            catch(Exception ex)
            {
                var exc = ex;
            }
            //
            //Retrieve changed data and perform custo processing on ViewModel Projection
            var finalData = await ctx.TestModels.Project().To<Models.TestViewModel>(m => new Models.TestViewModel {
                FieldBC=m.FieldB+" "+m.FieldC
            }).ToArrayAsync();
            return View();
        }
    }
}