using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MvcControlsToolkit.Core.Views
{
    public class RenderingScope : IDisposable
    {
        protected string oldPrefix;
        protected ViewDataDictionary viewData;
        protected object model;
        const string field = "__current_prefix__";
        internal static string Field {get { return field; } }
        public RenderingScope(object model, string newPrefix, ViewDataDictionary viewData)
        {
            if (newPrefix == null) throw new ArgumentNullException(nameof(newPrefix));
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            this.model = model;
            this.viewData = viewData;
            oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
            viewData.TemplateInfo.HtmlFieldPrefix = newPrefix;
            viewData[Field] = this;
        }

        public void Dispose()
        {
            viewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            viewData.Remove(Field);
        }
        public string Prefix { get { return viewData.TemplateInfo.HtmlFieldPrefix; } }
    }
    public class RenderingScope<T>: RenderingScope
    {
        
        
        public RenderingScope(T model, string newPrefix, ViewDataDictionary viewData)
            :base(model, newPrefix, viewData)
        {
           
        }       
        public T Model { get { return model == null ? default(T) :  (T)model; } }         
    }
}
