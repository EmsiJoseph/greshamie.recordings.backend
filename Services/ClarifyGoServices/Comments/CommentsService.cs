using backend.Models;
using backend.Services.Auth;
using IdentityModel.Client;
using System.Net.Http.Json;

namespace backend.Services.Comments
{
    public class CommentsService : ICommentsService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public CommentsService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(string recordingId, bool isLiveRecording)
        {
            // Retrieve access token and apply it as a Bearer token
            var accessToken = await _authService.GetAccessTokenAsync();
            _httpClient.SetBearerToken(accessToken);

            // Select the appropriate endpoint based on the recording type using swagger endpoints
            string endpoint = isLiveRecording
                ? $"/v1.0/liverecordings/{recordingId}/comments"
                : $"/v1.0/historicrecordings/{recordingId}/comments";

            // Execute the GET request
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            // Deserialize and return the comments
            return await response.Content.ReadFromJsonAsync<Comment[]>();
        }

        public async Task AddCommentAsync(string recordingId, string comment, bool isLiveRecording)
        {
            // Retrieve access token and apply it as a Bearer token
            var accessToken = await _authService.GetAccessTokenAsync();
            _httpClient.SetBearerToken(accessToken);

            // Select the correct endpoint for adding a comment based on the recording type
            string endpoint = isLiveRecording
                ? $"/v1.0/liverecordings/{recordingId}/comments"
                : $"/v1.0/historicrecordings/{recordingId}/comments";

            // Create the payload; assumes the API expects a JSON object with a "comment" property
            var payload = new { comment };

            // Execute the POST request to add the comment
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
