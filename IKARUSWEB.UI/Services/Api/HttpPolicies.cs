using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
namespace IKARUSWEB.UI.Services.Api
{
    public static class HttpPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(200), retryCount: 3);
            return HttpPolicyExtensions
                .HandleTransientHttpError()            // 5xx, 408, network errors
                .OrResult(r => (int)r.StatusCode == 429)
                .WaitAndRetryAsync(delay);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
        }
    }
}
