using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MvcControlsToolkit.Core.Extensions
{
    public class DIMetaDefault: IDIMeta
    {

        IDictionary<Type, Type> register = new Dictionary<Type, Type>();
        public DIMetaDefault (IServiceCollection services)
        {
            foreach(var s in services)
            {

                var fType = s.ServiceType.GetTypeInfo();
                if(fType.IsAbstract || fType.IsInterface || s.ImplementationFactory != null || s.ImplementationInstance != null)
                    register[s.ServiceType] = s.ImplementationType;
            }
        }

        public bool IsRegistred(Type t)
        {
            return register.ContainsKey(t);
        }

        public Type RegistredTypeFor(Type t)
        {
            return register[t];
        }
    }
}
