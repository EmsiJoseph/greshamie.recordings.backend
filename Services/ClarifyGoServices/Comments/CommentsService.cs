using System.Net;
using System.Text.Json;
using backend.Services.Auth;
using backend.ClarifyGoClasses;
using backend.Constants.ClarifyGo;
using backend.Exceptions;

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
            try
            {
                await _tokenService.SetBearerTokenAsync(_httpClient);

                var endpoint = isLiveRecording
                    ? ClarifyGoApiEndpoints.LiveRecordings.GetComments(recordingId)
                    : ClarifyGoApiEndpoints.HistoricRecordings.GetComments(recordingId);

                var response = await _httpClient.GetAsync(endpoint);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ServiceException("Unauthorized access to comments", 401);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new ServiceException($"Comments service error: {error}", (int)response.StatusCode);
                }

                return await response.Content.ReadFromJsonAsync<Comment[]>() ?? [];
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

        public async Task PostCommentAsync(string recordingId, string comment, bool isLiveRecording = false)
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

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
            await _tokenService.SetBearerTokenAsync(_httpClient);

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