using CryptoWise.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CryptoWise.API.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly IList<Role> _roles;

    public AuthorizeAttribute(params Role[]? roles)
    {
        _roles = roles ?? Array.Empty<Role>();
    }


    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        /*var account = (Account)context.HttpContext.Items["Account"];
        if (account == null || (_roles.Any() && !_roles.Contains(account.Role)))
        {
            context.Result = new JsonResult(new { message = "Unathorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
        }*/

        if (context.HttpContext.Items["Account"] is not Account account || (_roles.Any() && !_roles.Contains(account.Role)))
        {
            context.Result = new JsonResult(new { message = "Unathorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}