using backend.Constants;
using backend.Models;
using backend.Services.Auth;
using IdentityModel.Client;

namespace backend.Services.LiveRecordings;

public class LiveRecordingsService : ILiveRecordingsService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public LiveRecordingsService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<IEnumerable<LiveRecording>> GetLiveRecordingsAsync()
    {
        var accessToken = await _authService.GetAccessTokenAsync();
        _httpClient.SetBearerToken(accessToken);

        var response = await _httpClient.GetAsync(ClarifyGoApiEndpoints.LiveRecordings.GetAll);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<LiveRecording[]>();
    }

    public async Task ResumeRecordingAsync(string recorderId, string recordingId)
    {
        var accessToken = await _authService.GetAccessTokenAsync();
        _httpClient.SetBearerToken(accessToken);

        var endpoint = ClarifyGoApiEndpoints.LiveRecordings.Resume
            .Replace("{recorderId}", recorderId)
            .Replace("{recordingId}", recordingId);

        var response = await _httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task PauseRecordingAsync(string recorderId, string recordingId)
    {
        var accessToken = await _authService.GetAccessTokenAsync();
        _httpClient.SetBearerToken(accessToken);

        var endpoint = ClarifyGoApiEndpoints.LiveRecordings.Pause
            .Replace("{recorderId}", recorderId)
            .Replace("{recordingId}", recordingId);

        var response = await _httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
    }
}