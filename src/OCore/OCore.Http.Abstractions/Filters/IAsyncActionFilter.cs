using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace OCore.Http.Filters
{
    public interface IAsyncActionFilter 
    {        

        Task OnActionExecutionAsync(HttpContext context, Func<HttpContext, Task> next);

        float Order { get; }
    }
}
