using System.Text.Json;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;

namespace backend.Services.Auth;

public class DistributedTokenStorage : ITokenStorage
{
    private readonly IDistributedCache _cache;

    public DistributedTokenStorage(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task StoreTokenAsync(string sessionTokenId, TokenResponse token)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(15)
        };
        
        await _cache.SetStringAsync(
            $"token:{sessionTokenId}", 
            JsonSerializer.Serialize(token), 
            options);
    }

    public async Task<TokenResponse?> GetTokenAsync(string sessionTokenId)
    {
        var data = await _cache.GetStringAsync($"token:{sessionTokenId}");
        return data == null ? null : JsonSerializer.Deserialize<TokenResponse>(data);
    }

    public Task RemoveTokenAsync(string sessionTokenId) => 
        _cache.RemoveAsync($"token:{sessionTokenId}");
}