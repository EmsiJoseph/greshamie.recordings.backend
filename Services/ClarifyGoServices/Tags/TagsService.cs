using System.Net;
using System.Text.Json;
using backend.Services.Auth;
using backend.ClarifyGoClasses;
using backend.Constants.ClarifyGo;
using Microsoft.IdentityModel.Tokens;
using backend.Exceptions;

namespace backend.Services.ClarifyGoServices.Tags;

public class TagsService(HttpClient httpClient, ITokenService tokenService) : ITagsService
{
    private readonly HttpClient _httpClient = httpClient
                                              ?? throw new ArgumentNullException(nameof(httpClient));

    private readonly ITokenService _tokenService = tokenService
                                                   ?? throw new ArgumentNullException(nameof(tokenService));


    public async Task<IEnumerable<Tag>> GetTagsAsync(string recordingId, bool isLiveRecording = false)
    {
        try
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var endpoint = isLiveRecording
                ? ClarifyGoApiEndpoints.LiveRecordings.GetTags(recordingId)
                : ClarifyGoApiEndpoints.HistoricRecordings.GetTags(recordingId);

            var response = await _httpClient.GetAsync(endpoint);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ServiceException("Unauthorized access to tags", 401);
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ServiceException($"Tags service error: {error}", (int)response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<Tag[]>() ?? [];
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

    public async Task PostTagAsync(string recordingId, string tag, bool isLiveRecording = false)
    {
        await _tokenService.SetBearerTokenAsync(_httpClient);

        // Select the correct endpoint for adding a comment based on the recording type
        var endpoint = isLiveRecording
            ? ClarifyGoApiEndpoints.LiveRecordings.AddTag(recordingId, tag)
            : ClarifyGoApiEndpoints.HistoricRecordings.AddTag(recordingId, tag);

        // Execute the POST request to add the comment
        var response = await _httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTagAsync(string recordingId, string tag, bool isLiveRecording = false)
    {
        await _tokenService.SetBearerTokenAsync(_httpClient);

        // Select the correct endpoint for deleting a comment based on the recording type
        var endpoint = isLiveRecording
            ? ClarifyGoApiEndpoints.LiveRecordings.DeleteTag(recordingId, tag)
            : ClarifyGoApiEndpoints.HistoricRecordings.DeleteTag(recordingId, tag);


        // Execute the DELETE request to remove the comment
        var response = await _httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int limit)
    {
        await _tokenService.SetBearerTokenAsync(_httpClient);

        // Select the appropriate endpoint for retrieving the most used tags
        var endpoint = ClarifyGoApiEndpoints.Tags.MostUsed(limit);

        // Execute the GET request
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new SecurityTokenExpiredException("Access token has expired");
        }

        // Deserialize and return the tags
        return await response.Content.ReadFromJsonAsync<Tag[]>() ?? [];
    }
}