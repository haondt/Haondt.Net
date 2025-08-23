using Haondt.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Haondt.Web.Core.ModelBinders
{
    public class OptionalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;

            if (!modelType.IsGenericType || modelType.GetGenericTypeDefinition() != typeof(Optional<>))
                return Task.CompletedTask;

            var innerType = modelType.GetGenericArguments()[0];
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None || string.IsNullOrEmpty(valueProviderResult.FirstValue))
            {
                // No value found or value is an empty string, return an empty Optional<T>
                bindingContext.Result = ModelBindingResult.Success(Activator.CreateInstance(modelType));
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try
            {
                var value = valueProviderResult.FirstValue;
                object? convertedValue = Convert.ChangeType(value, innerType);

                var optionalInstance = Activator.CreateInstance(modelType, convertedValue);
                bindingContext.Result = ModelBindingResult.Success(optionalInstance);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
