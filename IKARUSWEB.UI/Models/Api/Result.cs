namespace IKARUSWEB.UI.Models.Api
{
    public sealed record Result<T>(bool Succeeded, string? Message, T? Data);
}
