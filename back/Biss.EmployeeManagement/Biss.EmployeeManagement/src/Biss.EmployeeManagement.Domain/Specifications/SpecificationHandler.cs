using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Domain.Specifications
{
    /// <summary>
    /// Handler para executar múltiplas specifications e consolidar resultados
    /// </summary>
    public class SpecificationHandler
    {
        /// <summary>
        /// Executa uma specification e retorna o resultado
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="specification">Specification a ser executada</param>
        /// <param name="entity">Entidade a ser validada</param>
        /// <returns>Resultado da validação</returns>
        public static SpecificationResult Validate<T>(ISpecification<T> specification, T entity)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            var isSatisfied = specification.IsSatisfiedBy(entity);
            
            return new SpecificationResult
            {
                IsValid = isSatisfied,
                Errors = isSatisfied ? new List<string>() : new List<string> { specification.ErrorMessage }
            };
        }

        /// <summary>
        /// Executa uma specification assíncrona
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="specification">Specification assíncrona a ser executada</param>
        /// <param name="entity">Entidade a ser validada</param>
        public static async Task ValidateAsync<T>(IAsyncSpecification<T> specification, T entity)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            await specification.IsSatisfiedByAsync(entity);
        }

        /// <summary>
        /// Executa múltiplas specifications e retorna o resultado consolidado
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="specifications">Lista de specifications a serem executadas</param>
        /// <param name="entity">Entidade a ser validada</param>
        /// <returns>Resultado consolidado da validação</returns>
        public static SpecificationResult ValidateAll<T>(IEnumerable<ISpecification<T>> specifications, T entity)
        {
            if (specifications == null || !specifications.Any())
                return new SpecificationResult { IsValid = true, Errors = new List<string>() };

            var results = specifications.Select(spec => Validate(spec, entity)).ToList();
            
            return new SpecificationResult
            {
                IsValid = results.All(r => r.IsValid),
                Errors = results.SelectMany(r => r.Errors).ToList()
            };
        }

        /// <summary>
        /// Executa múltiplas specifications assíncronas
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="specifications">Lista de specifications assíncronas a serem executadas</param>
        /// <param name="entity">Entidade a ser validada</param>
        public static async Task ValidateAllAsync<T>(IEnumerable<IAsyncSpecification<T>> specifications, T entity)
        {
            if (specifications == null || !specifications.Any())
                return;

            // Executar sequencialmente para evitar problemas de concorrência com DbContext
            foreach (var specification in specifications)
            {
                await ValidateAsync(specification, entity);
            }
        }
    }

    /// <summary>
    /// Resultado da execução de specifications
    /// </summary>
    public class SpecificationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        
        public string GetErrorMessage() => string.Join("; ", Errors);
    }
}
