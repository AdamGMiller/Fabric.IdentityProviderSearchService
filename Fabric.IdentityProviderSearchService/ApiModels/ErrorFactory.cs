using System;
using Nancy;

namespace Fabric.IdentityProviderSearchService.ApiModels
{
    public class ErrorFactory
    {

        public static Error CreateError<T>(string message, HttpStatusCode statusCode)
        {
            var error = new Error
            {
                Code = Enum.GetName(typeof(HttpStatusCode), statusCode),
                Target = typeof(T).Name,
                Message = message
            };
            return error;
        }
    }
}