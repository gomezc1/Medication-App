using System.Net;

namespace MedicationManager.Core.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when external API calls fail
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// HTTP status code from the API response
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Name of the API that failed
        /// </summary>
        public string ApiName { get; }

        public ApiException(string apiName)
            : base($"API call to {apiName} failed")
        {
            ApiName = apiName;
        }

        public ApiException(string apiName, string message)
            : base(message)
        {
            ApiName = apiName;
        }

        public ApiException(string apiName, string message, HttpStatusCode statusCode)
            : base(message)
        {
            ApiName = apiName;
            StatusCode = statusCode;
        }

        public ApiException(string apiName, string message, Exception innerException)
            : base(message, innerException)
        {
            ApiName = apiName;
        }

        public ApiException(string apiName, string message, HttpStatusCode statusCode, Exception innerException)
            : base(message, innerException)
        {
            ApiName = apiName;
            StatusCode = statusCode;
        }
    }
}