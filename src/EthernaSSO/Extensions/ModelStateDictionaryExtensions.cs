using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;

namespace Etherna.SSOServer.Extensions
{
    public static class ModelStateDictionaryExtensions
    {
        public static void ClearFieldErrors(this ModelStateDictionary modelState, params string[] fieldNames)
        {
            if (modelState is null)
                throw new ArgumentNullException(nameof(modelState));

            foreach (var field in from field in modelState
                                  where field.Value.ValidationState == ModelValidationState.Invalid
                                  where fieldNames.Select(f => f + ".")
                                                  .Any(f => field.Key.StartsWith(f, StringComparison.InvariantCulture))
                                  select field)
                field.Value.ValidationState = ModelValidationState.Valid;
        }
    }
}
