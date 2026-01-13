using Biss.EmployeeManagement.Domain.Entities.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Globalization;
using System.Net;

namespace Biss.EmployeeManagement.Api.Helper
{
    public class BaseControllerHandle : ControllerBase
    {
        const string TotalCountHeader = "X-Total-Count";
        const string NullResponseMessage = "Null response received.";
        const string BadRequestMessage = "Bad Request: {@Response}";
        const string PartialContentMessage = "Partial Content: {@Response}";
        const string NoContentMessage = "No Content: {@Response}";
        const string InternalServerErrorMessage = "An error occurred: {@Message}";

        protected readonly ILogger Logger;

        public BaseControllerHandle(ILogger logger)
        {
            Logger = logger;
        }

        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var acceptLanguage = context.HttpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                try
                {
                    var culture = new CultureInfo(acceptLanguage);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
                catch (CultureNotFoundException)
                {
                    Logger.LogWarning("Invalid culture provided: {AcceptLanguage}", acceptLanguage);
                }
            }
        }

        [NonAction]
        public void OnActionExecuted(ActionExecutedContext context) { }

        public ActionResult HandleResponse<TEntityResponse>(ApiResponse<TEntityResponse> response)
        {
            if (response == null)
            {
                Logger.LogInformation(NullResponseMessage);
                return BadRequest(NullResponseMessage);
            }

            if (!response.Success)
            {
                Logger.LogInformation(BadRequestMessage, response.Error?.Message);
                return BadRequest(response);
            }

            if (response.Data == null || response.Data.Response == null)
            {
                Logger.LogInformation(NoContentMessage, response.Error?.Message);
                return NoContent();
            }

            if (response.Data.Response is ICollection collection)
            {
                if (collection.Count == 0)
                {
                    Logger.LogInformation(NoContentMessage, response.Error?.Message);
                    return NoContent();
                }

                Response.Headers.Append(TotalCountHeader, collection.Count.ToString());
                Logger.LogInformation(PartialContentMessage, response.Error?.Message);
                return new ObjectResult(response) { StatusCode = (int)HttpStatusCode.PartialContent };
            }

            return response.StatusCode switch
            {
                (int)HttpStatusCode.Created => new ObjectResult(response) { StatusCode = (int)HttpStatusCode.Created },
                (int)HttpStatusCode.NoContent => new ObjectResult(response) { StatusCode = (int)HttpStatusCode.NoContent },
                (int)HttpStatusCode.NotFound => new ObjectResult(response) { StatusCode = (int)HttpStatusCode.NotFound },
                _ => Ok(response)
            };
        }

        protected ActionResult HandleException(Exception ex)
        {
            Logger.LogError(ex, InternalServerErrorMessage, ex.Message);
            return new ObjectResult(new ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)) 
            { 
                StatusCode = (int)HttpStatusCode.InternalServerError 
            };
        }
    }
}
