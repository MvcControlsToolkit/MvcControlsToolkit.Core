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
        protected string originalPrefix;
        protected string fatherPrefix;
        protected ViewDataDictionary viewData;
        protected object model;
        protected object options;
        const string field = "__current_prefix__";
        internal string FatherPrefix {get{ return fatherPrefix; } }
        internal static string Field { get { return field; } }
        internal object RawModel { get { return model; } }
        internal object Options { get { return options; } }
        private string subtractPrefix(string total, string part)
        {
            if (total == null || string.IsNullOrEmpty(part)) return total;
            var res = total.Substring(part.Length);
            if (res[0] == '.') res = res.Substring(1);
            return res;
        }
        private static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        public RenderingScope(object model, string newPrefix, ViewDataDictionary viewData, object options=null)
        {
            if (newPrefix == null) throw new ArgumentNullException(nameof(newPrefix));
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            this.model = model;
            this.options = options;
            this.viewData = viewData;
            oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
            viewData.TemplateInfo.HtmlFieldPrefix = newPrefix;
            var prev = viewData[Field] as RenderingScope;
            if (prev != null) originalPrefix = prev.originalPrefix;
            else originalPrefix = oldPrefix;
            viewData[Field] = this;
        }
        internal RenderingScope(object model, ViewDataDictionary viewData, string expression, object options = null)
        {
            
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            this.model = model;
            this.options = options;
            this.viewData = viewData;
            oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
            var prev = viewData[Field] as RenderingScope;
            if (prev != null)
            {
                viewData.TemplateInfo.HtmlFieldPrefix = prev.originalPrefix;
                fatherPrefix = combinePrefixes(subtractPrefix(oldPrefix, prev.originalPrefix), expression);
                originalPrefix = prev.originalPrefix;
            }
            else originalPrefix = oldPrefix;
            viewData[Field] = this;
        }
        public void Dispose()
        {
            viewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            viewData.Remove(Field);
        }
        public string Prefix { get { return viewData.TemplateInfo.HtmlFieldPrefix; } }

        public ScopeInfos<T> Infos<T>()
        {
            return new ScopeInfos<T>
            {
                Model = this.model == null ? default(T) : (T)this.model,
                Prefix = this.Prefix,
                FatherPrefix = this.fatherPrefix
            };
        }
        public ScopeInfos<T, O> TemplateInfos<T, O>()
            where O : class
        {
            return new ScopeInfos<T, O>
            {
                Model = this.model == null ? default(T):(T)this.model,
                Prefix = this.Prefix,
                FatherPrefix = this.fatherPrefix,
                Options = this.options as O
            };
        }
    }
    public class RenderingScope<T> : RenderingScope
    {


        public RenderingScope(T model, string newPrefix, ViewDataDictionary viewData)
            : base(model, newPrefix, viewData)
        {

        }
        public T Model { get { return model == null ? default(T) : (T)model; } }
        public ScopeInfos<T> Infos()
        {
            return new ScopeInfos<T>
            {
                Model = this.Model,
                Prefix = this.Prefix,
                FatherPrefix = this.fatherPrefix
            };
        }
        public ScopeInfos<T, O> TemplateInfos<O>()
            where O : class
        {
            return new ScopeInfos<T, O>
            {
                Model = this.Model,
                Prefix = this.Prefix,
                FatherPrefix = this.fatherPrefix,
                Options = this.options as O
            };
        }
    }
    public class ScopeInfos<T> 
    {
        public T Model { get; set; }
        public string Prefix { get; set; }
        public string FatherPrefix { get; set; }
    }
    public class ScopeInfos<T, O>: ScopeInfos<T>, IDisposable
        where O: class
    {
        public O Options { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Options = null;
                    Model = default(T);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScopeInfos() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}