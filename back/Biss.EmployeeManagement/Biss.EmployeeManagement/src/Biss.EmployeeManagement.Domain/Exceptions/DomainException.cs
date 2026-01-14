using System;
using System.Runtime.Serialization;

namespace Biss.EmployeeManagement.Domain.Exceptions
{
    [Serializable]
    public abstract class DomainException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        protected DomainException(string message, string errorCode, int statusCode = 400) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        protected DomainException(string message, string errorCode, int statusCode, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        protected DomainException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode)) ?? "UNKNOWN_ERROR";
            StatusCode = info.GetInt32(nameof(StatusCode));
        }

        [Obsolete("This method is obsolete and will be removed in future versions.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(StatusCode), StatusCode);
        }
    }
}
