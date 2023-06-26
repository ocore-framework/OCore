using System;
using System.Net;
using Orleans;

namespace OCore.Http
{
    [GenerateSerializer]
    public class StatusCodeException : Exception
    {
        public StatusCodeException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public StatusCodeException(HttpStatusCode statusCode,
            string message) : base(message)
        {
            StatusCode = statusCode;
        }
        
        public StatusCodeException(HttpStatusCode statusCode, 
            string message, 
            Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        [Id(0)]
        public HttpStatusCode StatusCode { get; set; }
    }
}
