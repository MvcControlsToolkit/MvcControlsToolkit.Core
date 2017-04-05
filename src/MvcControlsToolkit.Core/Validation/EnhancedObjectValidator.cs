using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;
using System.Reflection;

namespace MvcControlsToolkit.Core.Validation
{
    public class EnhancedObjectValidator: IObjectModelValidator
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ValidatorCache _validatorCache;
        private readonly IModelValidatorProvider _validatorProvider;

        
        public EnhancedObjectValidator(
            IModelMetadataProvider modelMetadataProvider,
            IList<IModelValidatorProvider> validatorProviders)
        {
            if (modelMetadataProvider == null)
            {
                throw new ArgumentNullException(nameof(modelMetadataProvider));
            }

            if (validatorProviders == null)
            {
                throw new ArgumentNullException(nameof(validatorProviders));
            }

            _modelMetadataProvider = modelMetadataProvider;
            _validatorCache = new ValidatorCache();

            _validatorProvider = new CompositeModelValidatorProvider(validatorProviders);
        }

        private Type referenceType(string prefix, Type modelType, ActionContext actionContext)
        {
            var parameterDescriptors = actionContext.ActionDescriptor.Parameters;
            ParameterDescriptor parameter;
            if (string.IsNullOrWhiteSpace(prefix))
                parameter = parameterDescriptors
                    .Where(m => m.ParameterType.GetTypeInfo().IsAssignableFrom(modelType))
                    .FirstOrDefault();
            else
            {
                parameter = parameterDescriptors
                    .Where(m => m.Name == prefix && m.ParameterType.GetTypeInfo().IsAssignableFrom(modelType))
                    .FirstOrDefault();
            }
            if (parameter != null && parameter.ParameterType.GetTypeInfo().IsInterface)
                return parameter.ParameterType;
            else return modelType;
        }
        public void Validate(
            ActionContext actionContext,
            ValidationStateDictionary validationState,
            string prefix,
            object model)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var visitor = new ValidationVisitor(
                actionContext,
                _validatorProvider,
                _validatorCache,
                _modelMetadataProvider,
                validationState);

            var metadata = model == null ? null : 
                _modelMetadataProvider.GetMetadataForType(referenceType(prefix, model.GetType(), actionContext));
            visitor.Validate(metadata, prefix, model);
        }
    }
}
