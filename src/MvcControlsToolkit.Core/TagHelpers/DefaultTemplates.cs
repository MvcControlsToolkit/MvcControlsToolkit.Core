using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public class DefaultTemplates
    {
        public Template<Column> DColumnTemplate {get; private set;}
        public Template<Column> EColumnTemplate { get; private set; }
        public Template<RowType> DRowTemplate { get; private set; }
        public Template<RowType> ERowTemplate { get; private set; }
        public Template<LayoutTemplateOptions> LayoutTemplate { get; private set; }
        public IEnumerable<Template<LayoutTemplateOptions>> LayoutParts { get; private set; }
        public bool IsDetail { get; private set; }
        public Func<IEnumerable<Column>, ContextualizedHelpers, object, IHtmlContent> RenderHiddenFields { get; private set; }
        
        public DefaultTemplates(
            bool isDetail,
            Template<Column> dCTemplate, 
            Template<Column> eCTemplate,
            Template<RowType> dRTemplate,
            Template<RowType> eRTemplate,
            Template<LayoutTemplateOptions> layoutTemplate,
            IEnumerable<Template<LayoutTemplateOptions>> layoutParts,
            Func<IEnumerable<Column>, ContextualizedHelpers, object, IHtmlContent> renderHiddenFields
            )
        {
            IsDetail = isDetail;
            DColumnTemplate = dCTemplate;
            EColumnTemplate = eCTemplate;
            DRowTemplate = dRTemplate;
            ERowTemplate = eRTemplate;
            LayoutTemplate = layoutTemplate;
            LayoutParts = layoutParts;
            RenderHiddenFields = renderHiddenFields;
            
        }
        public void Configure(DefaultTemplates changes)
        {
            if (changes.DColumnTemplate != null) DColumnTemplate = changes.DColumnTemplate;
            if (changes.DRowTemplate != null) DRowTemplate = changes.DRowTemplate;
            if (changes.EColumnTemplate != null) EColumnTemplate = changes.EColumnTemplate;
            if (changes.ERowTemplate != null) ERowTemplate = changes.ERowTemplate;
            if (changes.LayoutParts != null) LayoutParts = changes.LayoutParts;
            if (changes.LayoutTemplate != null) LayoutTemplate = changes.LayoutTemplate;
            if (changes.RenderHiddenFields != null) RenderHiddenFields = changes.RenderHiddenFields;
        }
        public Template<LayoutTemplateOptions>  GetLayoutTemplate(string partial)
        {
            if (string.IsNullOrEmpty(partial)) return LayoutTemplate;
            return new Template<LayoutTemplateOptions>(TemplateType.Partial, partial);
        }
        public IEnumerable<Template<LayoutTemplateOptions>> GetLayoutParts(IEnumerable<string> partials)
        {
            if (partials == null) foreach (var def in LayoutParts) yield return def;
            using (var enumeratorD = LayoutParts.GetEnumerator())
            using (var enumeratorC = partials.GetEnumerator())
            {
                var d = enumeratorD.MoveNext();
                var c = enumeratorC.MoveNext();
                while (d||c)
                {
                    if (c & d)
                    {
                        yield return enumeratorC.Current != null? 
                                new Template<LayoutTemplateOptions>(TemplateType.Partial, enumeratorC.Current)
                                :
                                enumeratorD.Current
                                ;
                        d = enumeratorD.MoveNext();
                        c = enumeratorC.MoveNext();
                    }
                    else if (c)
                    {
                        yield return
                                new Template<LayoutTemplateOptions>(TemplateType.Partial, enumeratorC.Current);
                        c = enumeratorC.MoveNext();
                    }
                    else
                    {
                        yield return enumeratorD.Current;
                        d = enumeratorD.MoveNext();
                    }
                }
            }
        }
    }
}
