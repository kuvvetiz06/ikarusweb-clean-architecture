using IKARUSWEB.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public sealed class CurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; }
    public Guid? UserId { get; }
    public string? UserName { get; }
    public Guid? TenantId { get; }
    public string? TenantName { get; }

    public CurrentUser(IHttpContextAccessor http)
    {
        var user = http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            IsAuthenticated = false;
            return;
        }

        IsAuthenticated = true;

        // UserId: "sub" veya NameIdentifier
        UserId = GetGuid(user,
            JwtRegisteredClaimNames.Sub,
            ClaimTypes.NameIdentifier
        );

        // UserName: Name / preferred_username / unique_name
        UserName = GetFirst(user,
            ClaimTypes.Name,
            "preferred_username",
            "unique_name"
        ) ?? user.Identity?.Name;

        // TenantId: "tenant_id" (senin tokenın böyle) + olası varyantlar
        TenantId = GetGuid(user,
            "tenant_id", "tenantId", "tid"
        );

        TenantName = GetFirst(user, "tenant_name");
    }

    private static string? GetFirst(ClaimsPrincipal u, params string[] types)
        => types.Select(t => u.FindFirst(t)?.Value).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private static Guid? GetGuid(ClaimsPrincipal u, params string[] types)
        => Guid.TryParse(GetFirst(u, types), out var g) ? g : (Guid?)null;
}
