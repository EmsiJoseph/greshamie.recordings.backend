using backend.Classes;
using backend.Constants;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public class HistoricRecordingsService : IHistoricRecordingsService
    {
        private readonly HttpClient _httpClient;

        public HistoricRecordingsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RecordingSearchResults> SearchRecordingsAsync(
            DateTime start,
            DateTime end,
            RecordingSearchFilters filters = null) // Updated parameter type
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.Search(start, end);

            var queryParams = BuildQueryParameters(filters);
            var fullUrl = queryParams.Any()
                ? $"{baseUrl}?{string.Join("&", queryParams)}"
                : baseUrl;

            var response = await _httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RecordingSearchResults>();
        }

        public async Task DeleteRecordingAsync(string recordingId)
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.Delete(recordingId);

            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> ExportMp3Async(string recordingId)
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportMp3(recordingId);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<Stream> ExportWavAsync(string recordingId)
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportWav(recordingId);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}