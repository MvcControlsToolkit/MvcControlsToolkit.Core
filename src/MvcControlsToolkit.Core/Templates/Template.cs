using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using MvcControlsToolkit.Core.Views;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MvcControlsToolkit.Core.Utilities;

namespace MvcControlsToolkit.Core.Templates
{
    public class Template<O>
    {
        
        public TemplateType Type { get; private set; }
        public Func<object, O, ContextualizedHelpers, IHtmlContent> FTemplate { get; private set; }
        public String TemplateName { get; private set; }
        protected AsyncLock Lock;
        protected ViewContext originalContext;
        protected const string viewLockName = "_view_lock_";
        public Template(TemplateType templateType, string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName)) throw new ArgumentNullException(nameof(templateName));
            if (templateType != TemplateType.Partial && templateType != TemplateType.ViewComponent)
                throw new ArgumentException(DefaultMessages.InconsistentTemplateName, nameof(templateType));
            Type = templateType;
            TemplateName = templateName;
        }
        private static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        public Template(TemplateType templateType, Func<object, O, ContextualizedHelpers, IHtmlContent> fTemplate, ViewContext originalContext)
        {
            if (fTemplate == null) throw new ArgumentNullException(nameof(fTemplate));
            if (templateType != TemplateType.InLine && templateType != TemplateType.Function)
                throw new ArgumentException(DefaultMessages.InconsistentTemplateFunc, nameof(templateType));
            Type = templateType;
            FTemplate = fTemplate;
            if (templateType == TemplateType.InLine)
            {
                if (originalContext == null) throw new ArgumentNullException(nameof(originalContext));
                this.originalContext = originalContext;
                Lock = originalContext.ViewData[viewLockName] as AsyncLock;
                if(Lock == null)
                {
                    originalContext.ViewData[viewLockName] = Lock = new AsyncLock();
                } 
            }
        }
        public async Task<IHtmlContent>  Invoke(ModelExpression expression, O options, ContextualizedHelpers helpers, string overridePrefix=null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (options == null) throw new ArgumentNullException(nameof(options));
            
            ModelExplorer model = expression.ModelExplorer;
            if (Type == TemplateType.Partial)
            {
                if (helpers == null) throw new ArgumentNullException(nameof(helpers));
                var h = helpers.Html;
                var origVd = helpers.Context.ViewData;
                if (h == null) throw new ArgumentNullException(nameof(h));
                var fatherPrefix = (origVd[RenderingScope.Field] as RenderingScope)?.FatherPrefix;
                var vd = new ViewDataDictionary<object>(origVd);
                vd.Model = model.Model;
                vd.ModelExplorer = model.GetExplorerForModel(model.Model);
                if(overridePrefix != null)
                    vd.TemplateInfo.HtmlFieldPrefix = combinePrefixes(overridePrefix,expression.Name);
                else
                    vd.TemplateInfo.HtmlFieldPrefix = origVd.GetFullHtmlFieldName(combinePrefixes(fatherPrefix, expression.Name));
                vd["Options"] = options;
                vd["ContextualizedHelpers"] = helpers;
                vd["LocalizerFactory"] = helpers.LocalizerFactory;
                vd.Remove(RenderingScope.Field);
                return await h.PartialAsync(TemplateName, model.Model, vd);
            }
            else if (Type == TemplateType.ViewComponent)
            {
                if (helpers == null) throw new ArgumentNullException(nameof(helpers));
                var origVd = helpers.Context.ViewData;
                var fatherPrefix = (origVd[RenderingScope.Field] as RenderingScope)?.FatherPrefix;
                return await helpers.Component.InvokeAsync(TemplateName, new {
                    model = model.Model,
                    options = options,
                    prefix = overridePrefix != null ? combinePrefixes(overridePrefix, expression.Name) : origVd.GetFullHtmlFieldName(combinePrefixes(fatherPrefix, expression.Name)),
                    modelState = origVd.ModelState, localizerFactory = helpers.LocalizerFactory, helpers=helpers });
                
            }
            else if (Type == TemplateType.InLine)
            {
                var res = helpers.GetCachedTemplateResult(this);
                if (res != null) return res;
                using (await Lock.LockAsync())
                {
                    originalContext.HttpContext = helpers.CurrentHttpContext;
                    
                    var origVd = originalContext.ViewData;
                    using (new RenderingScope(
                        expression.Model, 
                        overridePrefix != null? combinePrefixes(overridePrefix, expression.Name) : helpers.Context.ViewData.GetFullHtmlFieldName(expression.Name), 
                        origVd, 
                        options))
                    {
                        return FTemplate(model.Model, default(O), helpers);
                    }
                    
                }   
            }
            else
            {
                if (helpers == null) throw new ArgumentNullException(nameof(helpers));
                var origVd = helpers.Context.ViewData;
                using (new RenderingScope(
                    expression.Model, 
                    origVd,
                    expression.Name, 
                    options))
                {
                    return FTemplate(model.Model, options, helpers);
                }
            }
        }
    }
}
