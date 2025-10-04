using IKARUSWEB.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IKARUSWEB.Application.Behaviors;

public sealed class TenantScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<TenantScopeBehavior<TRequest, TResponse>> _logger;

    public TenantScopeBehavior(ICurrentUser currentUser,
                               ILogger<TenantScopeBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["TenantId"] = _currentUser.TenantId,
            ["UserId"] = _currentUser.UserId,
            ["UserName"] = _currentUser.UserName,
            ["TenantName"] = _currentUser.TenantName
        }))
        {
            return await next();
        }
    }
}
