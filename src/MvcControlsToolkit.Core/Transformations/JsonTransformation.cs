using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using MvcControlsToolkit.Core.Views;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MvcControlsToolkit.Core.Transformations
{
    public class JsonTransformation<T> : IBindingTransformation<T, string, T>
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
            IOptions<MvcJsonOptions> settings = currContext.RequestServices.GetService<IOptions<MvcJsonOptions>>();
            return (T)JsonConvert.DeserializeObject(x, typeof(T), settings.Value.SerializerSettings) ;
        }

        public string Transform(T x)
        {
            if (x == null) return "null";
            IOptions<MvcJsonOptions> settings = currContext.RequestServices.GetService<IOptions<MvcJsonOptions>>();
            return JsonConvert.SerializeObject(x, settings.Value.SerializerSettings);
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
            IOptions<MvcJsonOptions> settings = currContext.RequestServices.GetService<IOptions<MvcJsonOptions>>();
            return (T)JsonConvert.DeserializeObject(provider.CreateProtector("EncryptedJsonTransformation").Unprotect(x), typeof(T), settings.Value.SerializerSettings);
        }

        public string Transform(T x)
        {
            string res;
            if (x == null) res = "null";
            else
            {
                IOptions<MvcJsonOptions> settings = currContext.RequestServices.GetService<IOptions<MvcJsonOptions>>();
                res = JsonConvert.SerializeObject(x, settings.Value.SerializerSettings);
            }
            IDataProtectionProvider provider = currContext.RequestServices.GetService<IDataProtectionProvider>();
            return provider.CreateProtector("EncryptedJsonTransformation").Protect(res);
        }
    }
}
