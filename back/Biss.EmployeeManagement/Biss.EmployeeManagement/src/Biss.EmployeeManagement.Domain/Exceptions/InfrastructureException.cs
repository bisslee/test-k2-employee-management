using System;

namespace Biss.EmployeeManagement.Domain.Exceptions
{
    public class InfrastructureException : DomainException
    {
        public InfrastructureException(string message) 
            : base(message, "INFRASTRUCTURE_ERROR", 500)
        {
        }

        public InfrastructureException(string message, Exception innerException) 
            : base(message, "INFRASTRUCTURE_ERROR", 500, innerException)
        {
        }
    }

    public class DatabaseConnectionException : InfrastructureException
    {
        public DatabaseConnectionException(string message) 
            : base($"Database connection error: {message}")
        {
        }

        public DatabaseConnectionException(string message, Exception innerException) 
            : base($"Database connection error: {message}", innerException)
        {
        }
    }

    public class DatabaseOperationException : InfrastructureException
    {
        public DatabaseOperationException(string operation, string message) 
            : base($"Database {operation} error: {message}")
        {
        }

        public DatabaseOperationException(string operation, string message, Exception innerException) 
            : base($"Database {operation} error: {message}", innerException)
        {
        }
    }

    public class CacheException : InfrastructureException
    {
        public CacheException(string message) 
            : base($"Cache error: {message}")
        {
        }

        public CacheException(string message, Exception innerException) 
            : base($"Cache error: {message}", innerException)
        {
        }
    }
}
