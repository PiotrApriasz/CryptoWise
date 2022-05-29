using System.Net;
using System.Text.Json;
using CryptoWise.API.Exceptions;
using CryptoWise.Shared;

namespace CryptoWise.API.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception e)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            response.StatusCode = e switch
            {
                AppException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var result = JsonSerializer.Serialize(new BaseResponse
            {
                Error = true,
                Message = e?.Message
            });
            await response.WriteAsync(result);
        }
    }
}