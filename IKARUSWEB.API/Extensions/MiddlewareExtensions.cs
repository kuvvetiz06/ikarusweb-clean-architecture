using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace IKARUSWEB.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static WebApplication UseApiMiddlewares(this WebApplication app)
        {
            // Tenant middleware
            app.UseMultiTenant();

            // Localization
            // Localization
            var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("tr-TR") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });


            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            return app;
        }
    }
}