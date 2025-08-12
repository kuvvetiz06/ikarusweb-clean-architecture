using Microsoft.AspNetCore.Mvc;
using System.Net;
using FluentValidation;
namespace IKARUSWEB.API.Middlewares
{
    public sealed class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException vex)
            {
                var pd = new ProblemDetails
                {
                    Title = "Validation failed",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = string.Join(" | ", vex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
                };
                context.Response.StatusCode = pd.Status!.Value;
                await context.Response.WriteAsJsonAsync(pd);
            }
            catch (Exception ex)
            {
                var pd = new ProblemDetails
                {
                    Title = "Unexpected error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = appEnvDetail(ex)
                };
                context.Response.StatusCode = pd.Status!.Value;
                await context.Response.WriteAsJsonAsync(pd);
            }

            static string appEnvDetail(Exception ex) => ex.Message; // istersen env'e göre stack dönebilirsin
        }
    }
}
