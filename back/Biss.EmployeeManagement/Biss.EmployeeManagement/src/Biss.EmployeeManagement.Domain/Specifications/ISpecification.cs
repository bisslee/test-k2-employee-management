using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Domain.Specifications
{
    /// <summary>
    /// Interface base para specifications que validam regras de negócio
    /// </summary>
    /// <typeparam name="T">Tipo da entidade a ser validada</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Verifica se a specification é satisfeita pela entidade
        /// </summary>
        /// <param name="entity">Entidade a ser validada</param>
        /// <returns>True se a specification for satisfeita, false caso contrário</returns>
        bool IsSatisfiedBy(T entity);
        
        /// <summary>
        /// Mensagem de erro quando a specification não é satisfeita
        /// </summary>
        string ErrorMessage { get; }
    }

    /// <summary>
    /// Interface assíncrona para specifications que precisam de operações assíncronas
    /// </summary>
    /// <typeparam name="T">Tipo da entidade a ser validada</typeparam>
    public interface IAsyncSpecification<T>
    {
        /// <summary>
        /// Verifica se a specification é satisfeita pela entidade de forma assíncrona
        /// </summary>
        /// <param name="entity">Entidade a ser validada</param>
        /// <returns>True se a specification for satisfeita, false caso contrário</returns>
        Task<bool> IsSatisfiedByAsync(T entity);
        
        /// <summary>
        /// Mensagem de erro quando a specification não é satisfeita
        /// </summary>
        string ErrorMessage { get; }
    }
}
