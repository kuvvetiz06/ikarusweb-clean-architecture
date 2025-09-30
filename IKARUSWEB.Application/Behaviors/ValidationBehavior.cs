using FluentValidation;
using IKARUSWEB.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Behaviors
{

    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count == 0)
                return await next();

            // ❗ THROW YOK —> field -> messages sözlüğü
            var errors = failures
                .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "$" : e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage ?? "Validation error").ToArray()
                );

            // TResponse tipi Result<*> ise errors'lı Failure'ı çağır
            var t = typeof(TResponse);
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Result<*>.Failure(IDictionary<string,string[]>, string?)
                var failureOverload = t.GetMethod(nameof(Result<object>.Failure), new[] { typeof(IDictionary<string, string[]>), typeof(string) });
                if (failureOverload != null)
                {
                    var res = failureOverload.Invoke(null, new object[] { errors, "Doğrulama Hatası" });
                    return (TResponse)res!;
                }

                // overload bulunamazsa (teoride) property set fallback:
                var res2 = Activator.CreateInstance(t)!;
                t.GetProperty("Succeeded")?.SetValue(res2, false);
                t.GetProperty("Message")?.SetValue(res2, "Doğrulama Hatası");
                t.GetProperty("Errors")?.SetValue(res2, errors);
                return (TResponse)res2;
            }

            // Result<> değilse (ör. farklı bir dönüş tipi) zinciri bozmadan ilerle
            return await next();
        }
    }

}
