using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace MvcControlsToolkit.Core.ViewFeatures
{
    public class SafeMemoryPoolViewBufferScope : IViewBufferScope, IDisposable
    {
        private IViewBufferScope scopedBuffer = null;
        private ArrayPool<ViewBufferValue> viewBufferPool;
        private ArrayPool<char> charPool;
        private IServiceProvider provider;
        private List<IDisposable> toDispose;
        private IHttpContextAccessor contextAccessor;
        public SafeMemoryPoolViewBufferScope(ArrayPool<ViewBufferValue> viewBufferPool, ArrayPool<char> charPool, IHttpContextAccessor contextAccessor)
        {
            this.viewBufferPool = viewBufferPool;
            this.charPool = charPool;
            scopedBuffer = new MemoryPoolViewBufferScope(viewBufferPool, charPool);
            this.contextAccessor = contextAccessor; ;
        }
        private void addObjectToDispose(IDisposable x)
        {
            if (toDispose == null) toDispose = new List<IDisposable>();
            toDispose.Add(x);
        }
        private void ensureCreated()
        {
            if (scopedBuffer == null)
            {
                var buffer = contextAccessor.HttpContext.RequestServices.GetService(typeof(IViewBufferScope)) as SafeMemoryPoolViewBufferScope;
                if (buffer == null) return;
                scopedBuffer = buffer.scopedBuffer;
                buffer.addObjectToDispose(this);
            }
        }
        public PagedBufferedTextWriter CreateWriter(TextWriter writer)
        {
            ensureCreated();
            return scopedBuffer.CreateWriter(writer);
        }

        public void Dispose()
        {
            if(scopedBuffer != null)
            {
                if(scopedBuffer is IDisposable) (scopedBuffer as IDisposable).Dispose();
                scopedBuffer = null;
            }
            if(toDispose != null)
            {
                foreach (var x in toDispose) x.Dispose();
                toDispose = null;
            }
                
        }

        public ViewBufferValue[] GetPage(int pageSize)
        {
            ensureCreated();
            return scopedBuffer.GetPage(pageSize);
        }

        public void ReturnSegment(ViewBufferValue[] segment)
        {
            ensureCreated();
            scopedBuffer.ReturnSegment(segment);
        }
    }
}
