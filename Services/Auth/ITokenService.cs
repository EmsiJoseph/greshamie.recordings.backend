namespace backend.Services.Auth
{
    public interface ITokenService
    {
        string? GetAccessTokenFromContext();
        Task<string> GetAccessTokenAsync(string username, string password);
    }
}