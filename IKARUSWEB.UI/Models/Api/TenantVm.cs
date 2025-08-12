namespace IKARUSWEB.UI.Models.Api
{
    public sealed record TenantVm(
     Guid Id,
     string Name,
     string Street,
     string City,
     string Country,
     string DefaultCurrency,
     string TimeZone,
     string DefaultCulture
 );
}
