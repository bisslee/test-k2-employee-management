using Biss.EmployeeManagement.Application.Queries;

namespace Biss.EmployeeManagement.Application.Helpers
{
    public static class RequestPaginationHelper
    {
        public static int GetPage(int? page)
        {
            return page.HasValue && page.Value > 0 ? page.Value : 1;
        }

        public static int GetPageSize(int? pageSize)
        {
            return pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : 10;
        }

        public static T LoadPagination<T>(this T request) where T : BaseRequest
        {
            request.Page = GetPage(request.Page);
            request.Offset = GetPageSize(request.Offset);
            return request;
        }
    }
}
