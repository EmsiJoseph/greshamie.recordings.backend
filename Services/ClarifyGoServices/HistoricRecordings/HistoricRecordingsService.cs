using System.Net;
using backend.Classes;
using backend.Constants;
using backend.DTOs;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public class HistoricRecordingsService(HttpClient httpClient, ITokenService tokenService)
        : IHistoricRecordingsService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        private readonly ITokenService _tokenService =
            tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        private List<string> BuildQueryParameters(RecordingSearchFiltersDto? filters)
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
                    queryParams.Add($"ticks=0");
                }

                queryParams.Add($"minTimeOfDay={FormatTimeSpan(filters.EarliestTimeOfDay.Value)}");
                queryParams.Add($"maxTimeOfDay={FormatTimeSpan(filters.LatestTimeOfDay.Value)}");
            }

            // Boolean parameters
            AddQueryParam(queryParams, "hasScreenRecording", filters.HasScreenRecording);
            AddQueryParam(queryParams, "hasPciEvents", filters.HasPciComplianceEvents);
            AddQueryParam(queryParams, "hasRecordingEvaluation", filters.HasQualityEvaluation);
            AddQueryParam(queryParams, "filterRecordingByCompletionTime", filters.FilterByRecordingEndTime);
            AddQueryParam(queryParams, "searchUnallocatedDevices", filters.SearchUnallocatedDevices);
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
            AddQueryParam(queryParams, "huntGroupNumber", filters.HuntGroupNumber);
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

        private void AddQueryParam<T>(List<string> queryParams, string name, T? value = null) where T : struct
        {
            if (value.HasValue)
            {
                var stringValue = value switch
                {
                    bool b => b.ToString().ToLower(),
                    _ => value.Value.ToString()
                };

                stringValue ??= string.Empty;

                queryParams.Add($"{name}={Uri.EscapeDataString(stringValue)}");
            }
        }

        private void AddQueryParam(List<string> queryParams, string name, string? value)
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

        public async Task<IEnumerable<RecordingSearchResult>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto)
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);


            var baseUrl =
                ClarifyGoApiEndpoints.HistoricRecordings.Search(searchFiltersDto.StartDate, searchFiltersDto.EndDate);
            Console.WriteLine($"Base URL: {baseUrl}");

            var queryParams = BuildQueryParameters(searchFiltersDto);
            var fullUrl = queryParams.Any()
                ? $"{baseUrl}?{string.Join("&", queryParams)}"
                : baseUrl;

            Console.WriteLine($"Full URL: {fullUrl}");

            var response = await _httpClient.GetAsync(fullUrl);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new SecurityTokenExpiredException("Access token has expired");
            }

            response.EnsureSuccessStatusCode();

            // Deserialize into a RecordingSearchResults object.
            var searchResultsObj = await response.Content.ReadFromJsonAsync<RecordingSearchResults>();

            // Return the list of individual search results, or an empty list if null.
            return searchResultsObj?.SearchResults ?? new List<RecordingSearchResult>();
        }


        public async Task DeleteRecordingAsync(string recordingId)
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var url = ClarifyGoApiEndpoints.HistoricRecordings.Delete(recordingId);

            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> ExportMp3Async(string recordingId)
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportMp3(recordingId);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<Stream> ExportWavAsync(string recordingId)
        {
            await _tokenService.SetBearerTokenAsync(_httpClient);

            var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportWav(recordingId);

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}