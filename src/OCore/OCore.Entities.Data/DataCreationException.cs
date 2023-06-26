using System;

namespace OCore.Entities.Data
{
    public class DataCreationException : InvalidOperationException
    {
        public DataCreationException(string message) : base(message) { }
        public DataCreationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
