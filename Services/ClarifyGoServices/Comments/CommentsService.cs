using System.Net;
using backend.Services.Auth;
using IdentityModel.Client;
using backend.Classes;
using backend.Constants;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.ClarifyGoServices.Comments
{
    public class CommentsService(HttpClient httpClient, ITokenService tokenService) : ICommentsService
    {
        private readonly HttpClient _httpClient = httpClient
                                                  ?? throw new ArgumentNullException(nameof(httpClient));

        private readonly ITokenService _tokenService = tokenService
                                                       ?? throw new ArgumentNullException(nameof(tokenService));


        public async Task<IEnumerable<Comment>> GetCommentsAsync(string recordingId, bool isLiveRecording = false)
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
                ? ClarifyGoApiEndpoints.LiveRecordings.GetComments(recordingId)
                : ClarifyGoApiEndpoints.HistoricRecordings.GetComments(recordingId);

            // Execute the GET request
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new SecurityTokenExpiredException("Access token has expired");
            }

            // Deserialize and return the comments
            return await response.Content.ReadFromJsonAsync<Comment[]>() ?? [];
        }

        public async Task PostCommentAsync(string recordingId, string comment, bool isLiveRecording = false)
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
                ? ClarifyGoApiEndpoints.LiveRecordings.AddComment(recordingId, comment)
                : ClarifyGoApiEndpoints.HistoricRecordings.AddComment(recordingId, comment);

            // Create the payload; assumes the API expects a JSON object with a "comment" property
            var payload = new { comment };

            // Execute the POST request to add the comment
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCommentAsync(string recordingId, string commentId, bool isLiveRecording = false)
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
                ? ClarifyGoApiEndpoints.LiveRecordings.DeleteComment(recordingId, commentId)
                : ClarifyGoApiEndpoints.HistoricRecordings.DeleteComment(recordingId, commentId);

            // Execute the DELETE request to remove the comment
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }
    }
}