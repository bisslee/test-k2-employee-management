namespace Biss.EmployeeManagement.Domain.Constants
{
    public class DatabaseConfiguration
    {
        public bool EnableDetailedErrors { get; set; } = false;
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public int CommandTimeoutSeconds { get; set; } = 30;
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelaySeconds { get; set; } = 5;
    }
}
