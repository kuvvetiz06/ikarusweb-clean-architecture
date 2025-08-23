using IKARUSWEB.Application.Common.Security;


namespace IKARUSWEB.Application.Abstractions.Security
{
    public interface ITokenService
    {
        (string token, DateTimeOffset expiresAt) Create(UserTicket user);
    }
}
