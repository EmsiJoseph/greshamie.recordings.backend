using System.Net;
using backend.Constants;
using backend.Models;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.ClarifyGoServices.LiveRecordings;

public class LiveRecordingsService(HttpClient httpClient, ITokenService tokenService)
    : ILiveRecordingsService
{
    private readonly HttpClient _httpClient = httpClient
                                              ?? throw new ArgumentNullException(nameof(httpClient));

    private readonly ITokenService _tokenService = tokenService
                                                   ?? throw new ArgumentNullException(nameof(tokenService));

    public async Task<IEnumerable<Recording>> GetLiveRecordingsAsync()
    {
        var token = _tokenService.GetAccessTokenFromContext();

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Missing access token");
        }

        _httpClient.SetBearerToken(token);

        var response = await _httpClient.GetAsync(
            ClarifyGoApiEndpoints.LiveRecordings.GetAll());

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SecurityTokenExpiredException("Access token has expired");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<Recording>>() ?? Array.Empty<Recording>();
    }

    public async Task ResumeRecordingAsync(string recorderId, string recordingId)
    {
        var token = _tokenService.GetAccessTokenFromContext();

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Missing access token");
        }

        _httpClient.SetBearerToken(token);

        var endpoint = ClarifyGoApiEndpoints.LiveRecordings.Resume(recorderId, recordingId);

        var response = await _httpClient.PutAsync(endpoint, null);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SecurityTokenExpiredException("Access token has expired");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task PauseRecordingAsync(string recorderId, string recordingId)
    {
        var token = _tokenService.GetAccessTokenFromContext();


        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Missing access token");
        }

        _httpClient.SetBearerToken(token);
        
        var endpoint = ClarifyGoApiEndpoints.LiveRecordings.Pause(recorderId, recordingId);
        
        var response = await _httpClient.PutAsync(endpoint, null);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SecurityTokenExpiredException("Access token has expired");
        }

        response.EnsureSuccessStatusCode();
    }
}