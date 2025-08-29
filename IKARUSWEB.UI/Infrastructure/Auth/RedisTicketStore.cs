using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace IKARUSWEB.UI.Infrastructure.Auth;

public sealed class RedisTicketStore : ITicketStore
{
    private readonly IDistributedCache _cache;
    private readonly string _prefix;

    public RedisTicketStore(IDistributedCache cache, string instanceName = "ikarusweb:auth:")
    {
        _cache = cache;
        _prefix = instanceName;
    }

    private string Key(string key) => _prefix + key;

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var bytes = TicketSerializer.Default.Serialize(ticket);
        var props = ticket.Properties;

        var options = new DistributedCacheEntryOptions();
        if (props.ExpiresUtc.HasValue)
            options.AbsoluteExpiration = props.ExpiresUtc;
        else
            options.SlidingExpiration = TimeSpan.FromMinutes(60); // fallback

        await _cache.SetAsync(Key(key), bytes, options);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await _cache.GetAsync(Key(key));
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key) => _cache.RemoveAsync(Key(key));
}
