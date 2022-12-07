using CryptoWise.API.Authorization;
using CryptoWise.API.Helpers;
using Microsoft.Extensions.Options;

namespace CryptoWise.API.Middleware;

public class JwtMiddleware : IMiddleware
{
    private readonly AppSettings _appSettings;
    private readonly DataContext _dataContext;
    private readonly IJwtUtils _jwtUtils;
    
    public JwtMiddleware(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        if (token is not null)
        {
            var result = _jwtUtils.ValidateJwtToken(token);
            
            if (result != null)
            {
                if (result.Id is not null && result.Account is null)
                    context.Items["Account"] = await _dataContext.Accounts.FindAsync(result.Id);
                else
                {
                    context.Items["Account"] = result.Account;
                }
            }
        }
        
        await next(context);   
    }
}