using System.Net;
using System.Net.Http.Headers;

namespace IKARUSWEB.UI.Services.Api
{
    public sealed class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;

        public AuthTokenHandler(IHttpContextAccessor http) => _http = http;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = _http.HttpContext?.Request.Cookies["ik_token"];
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("API request unauthorized.");

            return response;
        }
    }
}
