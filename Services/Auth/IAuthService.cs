namespace backend.Services.Auth
{
    public interface IAuthService
    {
        Task<string> GetAccessTokenAsync();
    }
}