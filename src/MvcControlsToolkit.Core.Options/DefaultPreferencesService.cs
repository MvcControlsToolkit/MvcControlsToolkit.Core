using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace MvcControlsToolkit.Core.Options
{
    public class DefaultPreferencesService: IPreferencesService
    {
        private IOptionsDictionary optionsDictionary;
        private ProvidersDictionary providersInfos = new ProvidersDictionary();
        private static OptionObjectsDictionary optionsObjectsInfos = new OptionObjectsDictionary();
        private HttpContext requestContext;
        public void BindToRequest(HttpContext ctx)
        {
            requestContext = ctx;
            optionsDictionary.Creator = x =>
            {
                if (optionsObjectsInfos.ContainsKey(x))
                {
                    return ctx.RequestServices.GetService(x);
                }
                return null;
            };
        }

        public T BuildOptionsObject<T>()
             where T : class, new()
        {
            return optionsObjectsInfos.Bind<T>(optionsDictionary);
        }

        public DefaultPreferencesService(IOptionsDictionary optionsDictionary)
        {
            
            this.optionsDictionary = optionsDictionary;
        }


    }

}
