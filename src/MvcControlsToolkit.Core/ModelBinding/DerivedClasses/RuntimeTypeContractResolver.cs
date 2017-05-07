using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Extensions;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using MvcControlsToolkit.Core.Types;
using Newtonsoft.Json.Serialization;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class RuntimeTypeContractResolver: CamelCasePropertyNamesContractResolver
    {
        IDIMeta diMeta;
        IServiceProvider sp;
        public RuntimeTypeContractResolver(IDIMeta diMeta, IServiceProvider sp)
        {
            this.diMeta = diMeta;
            this.sp = sp;
        }
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
        
            if (diMeta.IsRegistred(objectType))
            {
                JsonObjectContract contract = DIResolveContract(objectType);
                contract.DefaultCreator = () => sp.GetService(objectType);

                return contract;
            }

            return base.CreateObjectContract(objectType);
        }
        private JsonObjectContract DIResolveContract(Type objectType)
        {
            var fType = diMeta.RegistredTypeFor(objectType);
            if (fType != null) return base.CreateObjectContract(fType);
            else return CreateObjectContract(objectType);
        }
        public  override JsonContract ResolveContract(Type type)
        {
            var res = base.ResolveContract(type);
            if(res.Converter == null)
            {
                if (DerivedClassesRegister.GetCodeFromType(type, true) != null)
                    res.Converter = new RuntimeTypesJsonConverter();
                
            }
            return res;
        }
    }
}
