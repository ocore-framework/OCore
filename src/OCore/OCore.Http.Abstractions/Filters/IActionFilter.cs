using Microsoft.AspNetCore.Http;

namespace OCore.Http.Filters
{
    public interface IActionFilter
    {
        void OnActionExecuting(HttpContext context);
        void OnActionExecuted(HttpContext context);
        float Order { get; }
    }
}
