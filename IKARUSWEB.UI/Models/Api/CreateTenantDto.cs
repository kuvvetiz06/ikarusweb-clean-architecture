namespace IKARUSWEB.UI.Models.Api
{
    public sealed record CreateTenantDto(
    string Name,
    string Country,
    string City,
    string Street,
    string DefaultCurrency,
    string TimeZone,
    string DefaultCulture
    );
}
