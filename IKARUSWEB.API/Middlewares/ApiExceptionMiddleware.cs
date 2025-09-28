using IKARUSWEB.API.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net;
using FluentValidation;

namespace IKARUSWEB.API.Middlewares
{
    public sealed class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IStringLocalizer<CommonResource> _LCommon;
        private readonly IStringLocalizer<ValidationResource> _LVal;

        public ApiExceptionMiddleware(
            RequestDelegate next,
            IStringLocalizer<CommonResource> lCommon,
            IStringLocalizer<ValidationResource> lVal)
        {
            _next = next;
            _LCommon = lCommon;
            _LVal = lVal;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (ValidationException ex)
            {
                // FV mesajlarını KEY olarak yazdık: e.ErrorMessage = "Validation.Name.Exists" gibi
                // Burada key -> localized metin'e çeviriyoruz.
                var errors = ex.Errors
                    // PropertyName boş olabilir (rule-level). Boşsa "$" altında toplayalım.
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "$" : e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e =>
                        {
                            var key = e.ErrorMessage ?? "Common.Validation";
                            var msg = _LVal[key].Value;
                            // Key resx'te yoksa orijinal mesajı kullan (dev'de teşhis kolay olur)
                            return string.IsNullOrWhiteSpace(msg) ? key : msg;
                        }).ToArray()
                    );

                var problem = new ValidationProblemDetails(errors)
                {
                    Title = _LCommon["Common.Validation"].Value, // "Doğrulama Hatası"
                    Status = StatusCodes.Status400BadRequest
                };

                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                ctx.Response.ContentType = "application/problem+json";
                await ctx.Response.WriteAsJsonAsync(problem);
                return; // önemli: pipeline'ı bitir
            }
        }
    }
}
