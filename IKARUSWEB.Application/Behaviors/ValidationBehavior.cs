using FluentValidation;
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

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (_validators.Any())
            {
                var ctx = new ValidationContext<TRequest>(request);
                var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, ct)));
                var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();
                if (failures.Count != 0)
                {
                    var message = string.Join(" | ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
                    throw new ValidationException(message, failures);
                }
            }
            return await next();
        }
    }
}
