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
            public abstract void Execute();  
        }
        protected class ContextRecord<T>: ContextRecord
        {
            public event Action<T> ContextClosing;
            public T Data { get; set; }
            public override void Execute()
            {
                if (ContextClosing != null) ContextClosing(Data);
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
        public void CloseContext()
        {
            if (Contexts == null || Contexts.Count == 0) return;
            var res = Contexts.Pop();
            res.Execute();
        }
        public void AttachEvent<T>(Action<T> action)
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
            res.CloseContext();
        }
        public static void AttachEvent<T>(HttpContext httpContext, string key, Action<T> action)
        {
            var res = Current(httpContext, key);
            if (res == null) return;
            res.AttachEvent(action);
        }
        public static void CloseContext<T>(HttpContext httpContext, string key, T data)
        {
            var res = Current(httpContext, key);
            if (res == null) return;
            res.ProvideEventData(data);
            res.CloseContext();
        }
        public static RenderingContext Current(HttpContext httpContext, string key)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            object res = null;
            if (httpContext.Items.TryGetValue(key, out res)) return res as RenderingContext;
            else return null;
        }


    }
}
