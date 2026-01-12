using Azure;
using Biss.EmployeeManagement.Domain.Entities.Response;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Collections;

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

        protected ILogger Logger { get; }

        public BaseControllerHandle(ILogger logger)
        {
            Logger = logger;
        }

        protected ApiErrorResponse GetModelStateError(ModelStateDictionary modelState)
        {
            var responseError = new ApiErrorResponse();
            List<string> errors = new();
            foreach (var state in modelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }

            string combinedErrors = string.Join(", ", errors);
            responseError.Message = combinedErrors;
            return responseError;
        }


        public ActionResult HandleResponse<TEntityResponse>
        (
            ApiResponse<TEntityResponse> response
        )

        {
            if (response == null)
            {
                Logger.LogInformation(NullResponseMessage);
                return BadRequest(NullResponseMessage);
            }

            if (!response.Success)
            {
                Logger.LogInformation(BadRequestMessage, response);
                return BadRequest(response);
            }

            if (response.Data == null || response.Data.Response == null)
            {
                Logger.LogInformation(NoContentMessage, response);
                return NoContent();
            }

            if (response.Data.Response is ICollection collection)
            {
                if (collection.Count == 0)
                {
                    Logger.LogInformation(NoContentMessage, response);
                    return NoContent();
                }

                Response.Headers.Append(TotalCountHeader, collection.Count.ToString());
                Logger.LogInformation(PartialContentMessage, response);
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
            var errorResponse = new ApiErrorResponse(ex);
            return new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }

}
