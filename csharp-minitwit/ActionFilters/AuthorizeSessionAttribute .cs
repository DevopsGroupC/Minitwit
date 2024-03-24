using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace csharp_minitwit.ActionFilters;

public class AsyncSessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        await Task.Run(() =>
        {
            var userId = context.HttpContext.Session.GetInt32("user_id");

            // If no user ID is found in the session, set the result to unauthorized
            if (!userId.HasValue)
            {
                context.Result = new UnauthorizedResult();
            }
        });
    }
}