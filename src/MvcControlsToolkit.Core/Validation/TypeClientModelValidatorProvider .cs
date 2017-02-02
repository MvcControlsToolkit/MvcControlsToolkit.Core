using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.Validation
{
    public class TypeClientModelValidatorProvider : IClientModelValidatorProvider
    {
        private static Type[] typesToValidate = new Type[] { typeof(double), typeof(float), typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(TimeSpan), typeof(DateTime), typeof(Week), typeof(Month), typeof(DateTimeOffset) };

        public void CreateValidators(ClientValidatorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var typeToValidate = context.ModelMetadata.UnderlyingOrModelType;
            if (typesToValidate.Contains(typeToValidate))
            {
                for (var i = 0; i < context.Results.Count; i++)
                {
                    var validator = context.Results[i].Validator;
                    if (validator != null && validator is TypeClientModelValidator)
                    {
                        // A validator is already present. No need to add one.
                        return;
                    }
                }

                context.Results.Add(new ClientValidatorItem
                {
                    Validator = new TypeClientModelValidator(),
                    IsReusable = true
                });
                
            }
        }
    }
}
