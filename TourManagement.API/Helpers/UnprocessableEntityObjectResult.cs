using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace TourManagement.API.Helpers
{
    public class UnprocessableEntityObjectResult : ObjectResult
    {
        /*
         * TODO: Return 422 status code - Unprocessable entity. We return a customized entity that is a dictionary<keyProperty, IList<CustomizedValidationError>> ==> IList<CustomizedValidationError>> = IList<keyValidation, message>
         * 
         * Example:
         * dictionary<name, list<validationKey,message>() {  {"required", "the name is required"}, {"maxLenght", "the name lenght is too long"} }>
         * 
         */
        public UnprocessableEntityObjectResult(ModelStateDictionary modelState)
            : base(new CustomizedValidationResult(modelState))
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }
            StatusCode = 422;
        }
    }
}
