namespace Biss.EmployeeManagement.Domain.Entities.Enums
{
    /// <summary>
    /// Roles dos funcionários com hierarquia
    /// Ordem: Director > Manager > Analyst > Assistant
    /// </summary>
    public enum EmployeeRole
    {
        Director = 1,
        Manager = 2,
        Analyst = 3,
        Assistant = 4
    }
}
