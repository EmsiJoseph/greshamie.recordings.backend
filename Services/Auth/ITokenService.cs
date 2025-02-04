namespace backend.Services.Auth
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}