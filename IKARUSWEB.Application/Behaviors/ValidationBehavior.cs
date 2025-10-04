using FluentValidation;
using MediatR;

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

            var ctx = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, ct))))
                           .SelectMany(r => r.Errors).Where(e => e is not null).ToList();

            if (failures.Count == 0) return await next();

            // field -> [KEY, KEY, ...]
            var dict = failures
                .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "$" : e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage ?? "Validation.Generic").ToArray());

            var t = typeof(TResponse);
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var m = t.GetMethod(nameof(Result<object>.Validation), new[] { typeof(IDictionary<string, string[]>), typeof(string) });
                var res = m!.Invoke(null, new object[] { dict, "Common.Validation" /* genel başlık KEY */ });
                return (TResponse)res!;
            }

            return await next();
        }
    }


}
