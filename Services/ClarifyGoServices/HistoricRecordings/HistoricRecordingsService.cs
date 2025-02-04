using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using backend.Constants;
using backend.Models;

namespace backend.Services.HistoricRecordings
{
    public class HistoricRecordingsService : IHistoricRecordingsService
    {
        private readonly HttpClient _httpClient;

        public HistoricRecordingsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RecordingSearchResults> SearchRecordingsAsync(DateTime start, DateTime end)
        {
            var formattedStart = Uri.EscapeDataString(start.ToUniversalTime().ToString("O"));
            var formattedEnd = Uri.EscapeDataString(end.ToUniversalTime().ToString("O"));

            var url = ClarifyGoApiEndpoints.HistoricRecordings.Search
                .Replace("{startDate}", formattedStart)
                .Replace("{endDate}", formattedEnd);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RecordingSearchResults>();
        }

        public async Task DeleteRecordingAsync(string recordingId)
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.Delete
                .Replace("{recordingId}", recordingId);

            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> ExportMp3Async(string recordingId)
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportMp3
                .Replace("{recordingId}", recordingId);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<Stream> ExportWavAsync(string recordingId)
        {
            var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportWav
                .Replace("{recordingId}", recordingId);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}