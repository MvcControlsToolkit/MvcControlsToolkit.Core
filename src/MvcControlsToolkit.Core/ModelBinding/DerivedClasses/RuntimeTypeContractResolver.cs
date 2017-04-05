using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using MvcControlsToolkit.Core.Types;
using Newtonsoft.Json.Serialization;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class RuntimeTypeContractResolver: CamelCasePropertyNamesContractResolver
    {
        public override JsonContract ResolveContract(Type type)
        {
            var res = base.ResolveContract(type);
            if(DerivedClassesRegister.GetCodeFromType(type, true) != null 
                && res.Converter == null)
                res.Converter = new RuntimeTypesJsonConverter();
            //else
            //{
            //    type = Nullable.GetUnderlyingType(type) ?? type;
            //    if (type == typeof(Month)) res.Converter = new MonthJsonConverter();
            //    else if (type == typeof(Week)) res.Converter = new WeekJsonConverter();
            //}
            return res;
        }
    }
}
