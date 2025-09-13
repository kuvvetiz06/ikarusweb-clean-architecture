using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace IKARUSWEB.UI.Infrastructure.Proxy
{
    public sealed class AccessTokenTransform : ITransformProvider
    {
        public void Apply(TransformBuilderContext context)
        {
            context.AddRequestTransform(async tfCtx =>
            {
                var httpCtx = tfCtx.HttpContext;
                var token = httpCtx.Session.GetString("access_token"); // PR-0018: Session
                if (!string.IsNullOrEmpty(token))
                {
                    tfCtx.ProxyRequest.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            });
        }

        public void ValidateCluster(TransformClusterValidationContext context) { }
        public void ValidateRoute(TransformRouteValidationContext context) { }
    }
}
