using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.Validation
{
    public class TypeClientModelValidatorProvider : IClientModelValidatorProvider
    {
        private static Type[] typesToValidate = new Type[] { typeof(double), typeof(float), typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(TimeSpan), typeof(DateTime), typeof(Week), typeof(Month)};
        public void GetValidators(ClientValidatorProviderContext context)
        {
            if(context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var typeToValidate = context.ModelMetadata.UnderlyingOrModelType;
            if (typesToValidate.Contains(typeToValidate))
            {
                context.Validators.Add(new TypeClientModelValidator());
            }
        }
    }
}
