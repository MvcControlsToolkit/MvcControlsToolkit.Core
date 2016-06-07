using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MvcControlsToolkit.Core.Options
{
    public class DefaultPreferencesService: IPreferencesService
    {
        private IOptionsDictionary optionsDictionary;
        private ProvidersDictionary providersInfos = new ProvidersDictionary();
        internal static OptionObjectsDictionary OptionsObjectsInfos = new OptionObjectsDictionary();
        private HttpContext requestContext;
        public void BindToRequest(HttpContext ctx)
        {
            requestContext = ctx;
            optionsDictionary.Creator = x =>
            {
                if (OptionsObjectsInfos.ContainsKey(x))
                {
                    return ctx.RequestServices.GetService(x);
                }
                return null;
            };
        }

        public T BuildOptionsObject<T>()
             where T : class, new()
        {
            return OptionsObjectsInfos.Bind<T>(optionsDictionary, providersInfos, requestContext);
        }
        public void Save<T>(T options)
        {
            OptionsObjectsInfos.Save(options, optionsDictionary, requestContext);
        }
        public DefaultPreferencesService(IOptionsDictionary optionsDictionary)
        {
            
            this.optionsDictionary = optionsDictionary;
        }


    }

}
