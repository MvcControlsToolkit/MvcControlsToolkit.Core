using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MvcControlsToolkit.Core.Views
{
    public class RenderingScope<T>: IDisposable
    {
        private T model;
        private string oldPrefix;
        private ViewDataDictionary viewData;
        public RenderingScope(T model, string newPrefix, ViewDataDictionary viewData)
        {
            if (newPrefix == null) throw new ArgumentNullException(nameof(newPrefix));
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            this.model = model;
            this.viewData = viewData;
            oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
            viewData.TemplateInfo.HtmlFieldPrefix = newPrefix;
        }

        public void Dispose()
        {
            viewData.TemplateInfo.HtmlFieldPrefix=oldPrefix;
        }

        public static T operator !(RenderingScope<T> x)
        {
            return x.model;
        }
        public T M(){
            return model;
        }
    }
}
