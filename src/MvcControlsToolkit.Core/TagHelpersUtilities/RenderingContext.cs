using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MvcControlsToolkit.Core.TagHelpersUtilities
{
    public class RenderingContext
    {
        protected abstract class ContextRecord
        {
            public abstract void Execute(object o);  
        }
        protected class ContextRecord<T>: ContextRecord
        {
            public event Action<T,object> ContextClosing;
            public T Data { get; set; }
            public override void Execute(object o)
            {
                if (ContextClosing != null) ContextClosing(Data, o);
            }
            public ContextRecord(T data)
            {
                Data = data;
            }
        }
        protected string _Key;
        public string Key { get { return _Key; } }

        protected Stack<ContextRecord> Contexts;
        public void OpenContext<T>(T data)
        {
            if (Contexts == null) Contexts = new Stack<ContextRecord>();
            Contexts.Push(new ContextRecord<T>(data));
        }
        public void CloseContext(object o)
        {
            if (Contexts == null || Contexts.Count == 0) return;
            var res = Contexts.Pop();
            res.Execute(o);
        }
        public bool Empty {get { return Contexts == null || Contexts.Count == 0; } }
        public void AttachEvent<T>(Action<T, object> action)
        {
            if (action == null) return;
            if (Contexts == null || Contexts.Count == 0) return;
            var record = Contexts.Peek() as ContextRecord<T>;
            if (record == null) return;
            record.ContextClosing += action;
        }
        public void ProvideEventData<T>(T data)
        {
            if (data == null) return;
            if (Contexts == null || Contexts.Count == 0) return;
            var record = Contexts.Peek() as ContextRecord<T>;
            if (record == null) return;
            record.Data = data;
        }
        public RenderingContext(HttpContext httpContext, string key)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            _Key = key;
            httpContext.Items[key] = this;

        }
        public static void OpenContext<T>(HttpContext httpContext, string key, T data)
        {
            var res = Current(httpContext, key);
            if (res == null) res = new RenderingContext(httpContext, key);
            res.OpenContext(data);
        }
        public static void CloseContext(HttpContext httpContext, string key)
        {
            var res = Current(httpContext, key);
            if (res == null) return;
            res.CloseContext(null);
        }
        public static void CloseContext(object o, HttpContext httpContext, string key)
        {
            var res = Current(httpContext, key);
            if (res == null) return;
            res.CloseContext(o);
        }
        public static void AttachEvent<T>(HttpContext httpContext, string key, Action<T, object> action)
        {
            var res = Current(httpContext, key);
            if (res == null) return;
            res.AttachEvent(action);
        }
        public static void CloseContext<T>(HttpContext httpContext, string key, T data, object o=null)
        {
            var res = Current(httpContext, key);
            if (res == null) return;
            res.ProvideEventData(data);
            res.CloseContext(o);
        }
        public static RenderingContext Current(HttpContext httpContext, string key)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            object res = null;
            if (httpContext.Items.TryGetValue(key, out res)) return res as RenderingContext;
            else return null;
        }
        public static T CurrentData<T>(HttpContext httpContext, string key)
        {
            var ctx = Current(httpContext, key);
            if (ctx == null || ctx.Contexts == null || ctx.Contexts.Count == 0) return default(T);
            var res = (ctx.Contexts.Peek() as ContextRecord<T>);
            if (res == null) return default(T);
            return res.Data;
        }

    }
    
}
namespace MvcControlsToolkit.Core.TagHelpers
{
    public class DisabledPostFormContent : IDisposable
    {
        private HttpContext httpContext;
        private const string formDisabled = "__disabled_form_context__";
        public DisabledPostFormContent(HttpContext httpContext)
        {
            this.httpContext = httpContext;
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            httpContext.Items[formDisabled] = true;
        }
        public void Dispose()
        {
            httpContext.Items[formDisabled] = false;
        }
        internal static bool IsDisabled(HttpContext httpContext)
        {
            if (httpContext == null) return false;
            object obj;
            return httpContext.Items.TryGetValue(formDisabled, out obj) && (bool)(obj);
        }
    }
}
