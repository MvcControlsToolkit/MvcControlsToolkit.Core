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

namespace MvcControlsToolkit.Core.Templates
{
    public class Template<O>
    {
        
        public TemplateType Type { get; private set; }
        public Func<object, O, ContextualizedHelpers, IHtmlContent> FTemplate { get; private set; }
        public String TemplateName { get; private set; }
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
        public Template(TemplateType templateType, Func<object, O, ContextualizedHelpers, IHtmlContent> fTemplate)
        {
            if (fTemplate == null) throw new ArgumentNullException(nameof(fTemplate));
            if (templateType != TemplateType.InLine && templateType != TemplateType.Function)
                throw new ArgumentException(DefaultMessages.InconsistentTemplateFunc, nameof(templateType));
            Type = templateType;
            FTemplate = fTemplate;
        }
        public IHtmlContent Invoke(ModelExpression expression, O options, ContextualizedHelpers helpers)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (helpers == null) throw new ArgumentNullException(nameof(helpers));
            ModelExplorer model = expression.ModelExplorer;
            if (Type == TemplateType.Partial)
            {
                var h = helpers.Html;
                var origVd = helpers.Context.ViewData;
                if (h == null) throw new ArgumentNullException(nameof(h));
                var fatherPrefix = (origVd[RenderingScope.Field] as RenderingScope)?.FatherPrefix;
                var vd = new ViewDataDictionary<object>(origVd);
                vd.Model = model.Model;
                vd.ModelExplorer = model.GetExplorerForModel(model.Model);
                vd.TemplateInfo.HtmlFieldPrefix = origVd.GetFullHtmlFieldName(combinePrefixes(fatherPrefix,expression.Name));
                vd["Options"] = options;
                vd["LocalizerFactory"] = helpers.LocalizerFactory;
                vd.Remove(RenderingScope.Field);
                return h.Partial(TemplateName, model.Model, vd);
            }
            else if (Type == TemplateType.ViewComponent)
            {
                var origVd = helpers.Context.ViewData;
                var fatherPrefix = (origVd[RenderingScope.Field] as RenderingScope)?.FatherPrefix;
                var res = helpers.Component.InvokeAsync(TemplateName, new { model = model.Model, options = options, prefix = origVd.GetFullHtmlFieldName(combinePrefixes(fatherPrefix, expression.Name)),  modelState= origVd.ModelState, localizerFactory = helpers.LocalizerFactory });
                res.Wait();
                return res.Result;
            }
            else if (Type == TemplateType.InLine)
            {
                if(string.IsNullOrWhiteSpace(expression.Name) )
                    return FTemplate(model.Model, options, helpers);
                else
                {
                    var origVd = helpers.Context.ViewData;
                    using (new RenderingScope(expression.Model, origVd.GetFullHtmlFieldName(expression.Name), origVd, options))
                    {
                        return FTemplate(model.Model, default(O), helpers);
                    }
                }
            }
            else
            {
                var origVd = helpers.Context.ViewData;
                using (new RenderingScope(expression.Model, origVd, origVd.GetFullHtmlFieldName(expression.Name), options))
                {
                    return FTemplate(model.Model, default(O), helpers);
                }
            }
        }
    }
}
