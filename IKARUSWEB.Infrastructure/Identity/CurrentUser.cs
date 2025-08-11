using IKARUSWEB.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Infrastructure.Identity
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUser(IHttpContextAccessor http) => _http = http;

        public string? UserId =>
            _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _http.HttpContext?.User?.FindFirst("sub")?.Value;

        public string? UserName =>
            _http.HttpContext?.User?.Identity?.Name
            ?? _http.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public Guid? TenantId
        {
            get
            {
                var raw = _http.HttpContext?.User?.FindFirst("tenantId")?.Value;
                return Guid.TryParse(raw, out var id) ? id : null;
            }
        }
    }
}
