using Biss.EmployeeManagement.Domain.Entities.Response;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Helpers
{
    public class HandleResponseExceptionHelper
    {
        private readonly ILogger Logger;

        public HandleResponseExceptionHelper(ILogger logger)
        {
            Logger = logger;
        }

        public void HandleResponseException<T>(ApiResponse<T> response, Exception ex)
        {
            Logger.LogError(ex.Message);
            response.Error = new ApiErrorResponse(ex);
            response.StatusCode = 500;
        }


        public ApiResponse<T> HandleResponseBadRequest<T>(ApiResponse<T> response, StringBuilder message)
        {
            response.Error = new ApiErrorResponse();
            response.Error.Detail = message.ToString();
            response.Error.Message = "Invalid Request";
            response.Success = false;
            response.StatusCode = 400;
            return response;
        }
    }
}
