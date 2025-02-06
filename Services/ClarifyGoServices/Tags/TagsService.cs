using System.Net;
using backend.Services.Auth;
using IdentityModel.Client;
using backend.Classes;
using backend.Constants;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.ClarifyGoServices.Tags
{
    public class TagsService(HttpClient httpClient, ITokenService tokenService) : ITagsService
    {
        private readonly HttpClient _httpClient = httpClient
                                                  ?? throw new ArgumentNullException(nameof(httpClient));

        private readonly ITokenService _tokenService = tokenService
                                                       ?? throw new ArgumentNullException(nameof(tokenService));


        public async Task<IEnumerable<Tag>> GetTagsAsync(string recordingId, bool isLiveRecording = false)
        {
            // Retrieve access token and apply it as a Bearer token
            var token = _tokenService.GetAccessTokenFromContext();

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Missing access token");
            }

            _httpClient.SetBearerToken(token);

            // Select the appropriate endpoint based on the recording type using swagger endpoints
            var endpoint = isLiveRecording
                ? ClarifyGoApiEndpoints.LiveRecordings.GetTags(recordingId)
                : ClarifyGoApiEndpoints.HistoricRecordings.GetTags(recordingId);

            // Execute the GET request
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new SecurityTokenExpiredException("Access token has expired");
            }

            // Deserialize and return the comments
            return await response.Content.ReadFromJsonAsync<Tag[]>() ?? [];
        }

        public async Task PostTagAsync(string recordingId, string tag, bool isLiveRecording = false)
        {
            // Retrieve access token and apply it as a Bearer token
            var token = _tokenService.GetAccessTokenFromContext();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Missing access token");
            }

            _httpClient.SetBearerToken(token);

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
            // Retrieve access token and apply it as a Bearer token
            var token = _tokenService.GetAccessTokenFromContext();

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Missing access token");
            }

            _httpClient.SetBearerToken(token);

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
            // Retrieve access token and apply it as a Bearer token
            var token = _tokenService.GetAccessTokenFromContext();

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Missing access token");
            }

            _httpClient.SetBearerToken(token);

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
}