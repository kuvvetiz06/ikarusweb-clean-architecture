using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using Yarp.ReverseProxy.Forwarder;

namespace IKARUSWEB.UI.Infrastructure
{
    public sealed class RefreshCaptureTransformer : HttpTransformer
    {
        public override async ValueTask<bool> TransformResponseAsync(HttpContext ctx, HttpResponseMessage? proxyResponse, CancellationToken ct)
        {
            if ((int)proxyResponse.StatusCode is >= 200 and < 300 && proxyResponse.Content is not null)
            {
                var json = await proxyResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");
                var token = data.GetProperty("accessToken").GetString();
                var exp = data.GetProperty("expiresAt").GetDateTimeOffset();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    // 1) Session'a yeni access token
                    ctx.Session.SetString("access_token", token);

                    // 2) UI cookie süresini access token'a göre uzat
                    // (claim'leri mevcut principal'dan oku; sadece süresini güncelle)
                    var principal = ctx.User;
                    if (principal?.Identity?.IsAuthenticated == true)
                    {
                        //var props = new AuthenticationProperties
                        //{
                        //    IsPersistent = true,
                        //    ExpiresUtc = exp
                        //};

                        var props = new AuthenticationProperties
                        {
                            IsPersistent = false
                            // ExpiresUtc set ETME; sliding expiration yeterli
                        };
                        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
                    }

                    // Dönen JSON'u aynen geçir
                }
            }

            return await base.TransformResponseAsync(ctx, proxyResponse, ct);
        }
    }
}
