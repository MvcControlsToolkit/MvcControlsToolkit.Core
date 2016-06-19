using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using MvcControlsToolkit.Core.Views;
using Newtonsoft.Json;

namespace MvcControlsToolkit.Core.Transformations
{
    public class JsonTransformation<T> : IBindingTransformation<T, string, T>
    {
        public HttpContext Context
        {
            set
            {
                
            }
        }

        
        public T InverseTransform(string x)
        {
            if (x == null) return default(T);
            return (T)JsonConvert.DeserializeObject(x, typeof(T)) ;
        }

        public string Transform(T x)
        {
            if (x == null) return "null";
            return JsonConvert.SerializeObject(x);
        }
    }
    public class EncryptedJsonTransformation<T> : IBindingTransformation<T, string, T>
    {
        public HttpContext Context
        {
            set
            {
                currContext = value; ;
            }
        }

        private HttpContext currContext;
        public T InverseTransform(string x)
        {
            if (x == null) return default(T);
            IDataProtectionProvider provider = currContext.RequestServices.GetService<IDataProtectionProvider>();
            return (T)JsonConvert.DeserializeObject(provider.CreateProtector("EncryptedJsonTransformation").Unprotect(x), typeof(T));
        }

        public string Transform(T x)
        {
            string res;
            if (x == null) res = "null";
            else res= JsonConvert.SerializeObject(x);
            IDataProtectionProvider provider = currContext.RequestServices.GetService<IDataProtectionProvider>();
            return provider.CreateProtector("EncryptedJsonTransformation").Protect(res);
        }
    }
}
