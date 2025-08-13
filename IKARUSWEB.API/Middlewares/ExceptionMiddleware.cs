using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net;
namespace IKARUSWEB.API.Middlewares
{
    public sealed class ExceptionMiddleware : IMiddleware
    {
        private readonly IStringLocalizer _shared;
        private readonly IStringLocalizer _validation;

        public ExceptionMiddleware(
            IStringLocalizerFactory factory)
        {
            _shared = factory.Create("SharedResource", typeof(Program).Assembly.GetName().Name!);
            _validation = factory.Create("Validation", typeof(Program).Assembly.GetName().Name!);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException vex)
            {
                var errors = vex.Errors
                    .Select(e =>
                    {
                        // Önce ErrorCode ile çeviri dene; yoksa ErrorMessage’a düş
                        var msg = !string.IsNullOrWhiteSpace(e.ErrorCode)
                            ? _validation[e.ErrorCode].ResourceNotFound ? e.ErrorMessage : _validation[e.ErrorCode].Value
                            : e.ErrorMessage;

                        return new { field = e.PropertyName, code = e.ErrorCode, message = msg };
                    })
                    .ToList();

                var pd = new ProblemDetails
                {
                    Title = _shared["ValidationFailed"],
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = string.Join(" | ", errors.Select(x => $"{x.field}: {x.message}"))
                };

                context.Response.StatusCode = pd.Status!.Value;
                await context.Response.WriteAsJsonAsync(new { problem = pd, errors });
            }
            catch (Exception)
            {
                var pd = new ProblemDetails
                {
                    Title = _shared["UnexpectedError"],
                    Status = (int)HttpStatusCode.InternalServerError
                };
                context.Response.StatusCode = pd.Status!.Value;
                await context.Response.WriteAsJsonAsync(pd);
            }
        }
    }
}
