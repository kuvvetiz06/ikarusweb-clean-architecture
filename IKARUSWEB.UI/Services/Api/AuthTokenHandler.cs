using System.Net.Http.Headers;

namespace IKARUSWEB.UI.Services.Api
{
    public sealed class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;

        public AuthTokenHandler(IHttpContextAccessor http) => _http = http;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _http.HttpContext?.Request.Cookies["ik_token"];
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
