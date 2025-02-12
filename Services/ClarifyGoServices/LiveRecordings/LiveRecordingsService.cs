using System.Net;
using System.Text.Json;
using backend.Constants.ClarifyGo;
using backend.DTOs.Recording;
using backend.Services.Auth;
using backend.Exceptions;

namespace backend.Services.ClarifyGoServices.LiveRecordings;

public class LiveRecordingsService(HttpClient httpClient, ITokenService tokenService)
    : ILiveRecordingsService
{
    private readonly HttpClient _httpClient = httpClient
                                              ?? throw new ArgumentNullException(nameof(httpClient));

    private readonly ITokenService _tokenService = tokenService
                                                   ?? throw new ArgumentNullException(nameof(tokenService));

    public async Task<IEnumerable<RecordingDto>> GetLiveRecordingsAsync()
    {
        try
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var response = await _httpClient.GetAsync(ClarifyGoApiEndpoints.LiveRecordings.GetAll());

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ServiceException("Unauthorized access to live recordings", 401);
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ServiceException($"Live recording service error: {error}", (int)response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<RecordingDto>>()
                   ?? Array.Empty<RecordingDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Network error: {ex.Message}", 503);
        }
        catch (JsonException ex)
        {
            throw new ServiceException($"Invalid response format: {ex.Message}", 502);
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Unexpected error: {ex.Message}");
        }
    }

    public async Task ResumeRecordingAsync(string recorderId, string recordingId)
    {
        try
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var endpoint = ClarifyGoApiEndpoints.LiveRecordings.Resume(recorderId, recordingId);
            var response = await _httpClient.PutAsync(endpoint, null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ServiceException("Unauthorized access to resume recording", 401);
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ServiceException($"Resume recording error: {error}", (int)response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Network error: {ex.Message}", 503);
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Unexpected error: {ex.Message}");
        }
    }

    public async Task PauseRecordingAsync(string recorderId, string recordingId)
    {
        try
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var endpoint = ClarifyGoApiEndpoints.LiveRecordings.Pause(recorderId, recordingId);
            var response = await _httpClient.PutAsync(endpoint, null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ServiceException("Unauthorized access to pause recording", 401);
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ServiceException($"Pause recording error: {error}", (int)response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceException($"Network error: {ex.Message}", 503);
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Unexpected error: {ex.Message}");
        }
    }
}