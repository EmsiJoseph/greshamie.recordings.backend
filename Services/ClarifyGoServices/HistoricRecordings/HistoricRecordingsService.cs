using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using backend.Constants;
using backend.Models;
using backend.Services.HistoricRecordings;

namespace backend.Services.HistoricRecordings
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
            var formattedStart = Uri.EscapeDataString(start.ToUniversalTime().ToString("O"));
            var formattedEnd = Uri.EscapeDataString(end.ToUniversalTime().ToString("O"));

            var baseUrl = ClarifyGoApiEndpoints.HistoricRecordings.Search
                .Replace("{startDate}", formattedStart)
                .Replace("{endDate}", formattedEnd);

            var queryParams = BuildQueryParameters(filters);
            var fullUrl = queryParams.Any()
                ? $"{baseUrl}?{string.Join("&", queryParams)}"
                : baseUrl;

            var response = await _httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RecordingSearchResults>();
        }

        private List<string> BuildQueryParameters(RecordingSearchFilters filters)
        {
            var queryParams = new List<string>();

            if (filters == null)
            {
                return queryParams;
            }

            // Time parameters: Both EarliestTimeOfDay and LatestTimeOfDay must be provided together.
            if (filters.EarliestTimeOfDay.HasValue || filters.LatestTimeOfDay.HasValue)
            {
                if (!filters.EarliestTimeOfDay.HasValue || !filters.LatestTimeOfDay.HasValue)
                {
                    throw new ArgumentException("Both EarliestTimeOfDay and LatestTimeOfDay must be provided together");
                }

                queryParams.Add($"minTimeOfDay={FormatTimeSpan(filters.EarliestTimeOfDay.Value)}");
                queryParams.Add($"maxTimeOfDay={FormatTimeSpan(filters.LatestTimeOfDay.Value)}");
            }

            // Boolean parameters
            AddQueryParam(queryParams, "filterRecordingByCompletionTime", filters.FilterByRecordingEndTime);
            AddQueryParam(queryParams, "hasScreenRecording", filters.HasVideoRecording);
            AddQueryParam(queryParams, "hasPciEvents", filters.HasPciComplianceEvents);
            AddQueryParam(queryParams, "hasRecordingEvaluation", filters.HasQualityEvaluation);
            AddQueryParam(queryParams, "sortDescending", filters.SortDescending);

            // Numeric parameters
            AddQueryParam(queryParams, "minDurationSeconds", filters.MinimumDurationSeconds);
            AddQueryParam(queryParams, "maxDurationSeconds", filters.MaximumDurationSeconds);

            // Pagination parameters
            AddQueryParam(queryParams, "firstResultIndex", filters.PageOffset);
            AddQueryParam(queryParams, "maxResults", filters.PageSize);

            // Call metadata (String parameters)
            AddQueryParam(queryParams, "phoneNumber", filters.PhoneNumber);
            AddQueryParam(queryParams, "direction", filters.CallDirection);
            AddQueryParam(queryParams, "device", filters.DeviceNumber);
            AddQueryParam(queryParams, "huntGroupNumber", filters.HuntGroup);
            AddQueryParam(queryParams, "accountCode", filters.AccountCode);
            AddQueryParam(queryParams, "callId", filters.CallId);

            // Content filters
            AddQueryParam(queryParams, "commentText", filters.CommentContains);
            AddQueryParam(queryParams, "tag", filters.TagName);
            AddQueryParam(queryParams, "bookmark", filters.BookmarkText);

            // Recorder filters
            AddQueryParam(queryParams, "redundancyType", filters.RecorderType);
            AddQueryParam(queryParams, "recorderFilter", filters.RecorderId);

            // Sorting parameter
            AddQueryParam(queryParams, "sortBy", filters.SortBy);

            // Advanced Filters
            AddQueryParam(queryParams, "recordingGroupingId", filters.RecordingGroupId);

            return queryParams;
        }

        private void AddQueryParam<T>(List<string> queryParams, string name, T? value) where T : struct
        {
            if (value.HasValue)
            {
                var stringValue = value switch
                {
                    bool b => b.ToString().ToLower(),
                    _ => value.Value.ToString()
                };
                queryParams.Add($"{name}={Uri.EscapeDataString(stringValue)}");
            }
        }

        private void AddQueryParam(List<string> queryParams, string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                queryParams.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }

        private string FormatTimeSpan(TimeSpan time)
        {
            // Formats the TimeSpan as "HH:mm" (wrapped in quotes for the API)
            return Uri.EscapeDataString($"\"{time.Hours:D2}:{time.Minutes:D2}\"");
        }

        // Other methods remain the same
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
