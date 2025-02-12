using System.Net;
using System.Text.Json;
using backend.ClarifyGoClasses;
using backend.Constants;
using backend.DTOs;
using backend.Exceptions;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public class HistoricRecordingsService : IHistoricRecordingsService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public HistoricRecordingsService(HttpClient httpClient, ITokenService tokenService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public void SetBearerToken(string token)
        {
            _httpClient.SetBearerToken(token);
        }

        private List<string> BuildQueryParameters(RecordingSearchFiltersDto? filters)
        {
            var queryParams = new List<string>();

            if (filters == null)
            {
                return queryParams;
            }

            var clarifyGoFilters = new ClarifyGoSearchFilters();

            // Time parameters: Both EarliestTimeOfDay and LatestTimeOfDay must be provided together.
            if (filters.EarliestTimeOfDay.HasValue || filters.LatestTimeOfDay.HasValue)
            {
                if (!filters.EarliestTimeOfDay.HasValue || !filters.LatestTimeOfDay.HasValue)
                {
                    queryParams.Add($"ticks=0");
                }

                queryParams.Add($"{clarifyGoFilters.MinTimeOfDay}={FormatTimeSpan(filters.EarliestTimeOfDay.Value)}");
                queryParams.Add($"{clarifyGoFilters.MaxTimeOfDay}={FormatTimeSpan(filters.LatestTimeOfDay.Value)}");
            }

            // Boolean parameters
            AddQueryParam(queryParams, $"{clarifyGoFilters.HasScreenRecording}", filters.HasScreenRecording);
            AddQueryParam(queryParams, $"{clarifyGoFilters.HasPciEvents}", filters.HasPciComplianceEvents);
            AddQueryParam(queryParams, $"{clarifyGoFilters.HasRecordingEvaluation}", filters.HasQualityEvaluation);
            AddQueryParam(queryParams, $"{clarifyGoFilters.FilterRecordingByCompletionTime}",
                filters.FilterByRecordingEndTime);
            AddQueryParam(queryParams, $"{clarifyGoFilters.SearchUnallocatedDevices}",
                filters.SearchUnallocatedDevices);
            AddQueryParam(queryParams, $"{clarifyGoFilters.SortDescending}", filters.SortDescending);

            // Numeric parameters
            AddQueryParam(queryParams, $"{clarifyGoFilters.MinDurationSeconds}", filters.MinimumDurationSeconds);
            AddQueryParam(queryParams, $"{clarifyGoFilters.MaxDurationSeconds}", filters.MaximumDurationSeconds);

            // Pagination parameters
            AddQueryParam(queryParams, $"{clarifyGoFilters.FirstResultIndex}", filters.PageOffset);
            AddQueryParam(queryParams, $"{clarifyGoFilters.MaxResults}", filters.PageSize);

            // Call metadata (String parameters)
            AddQueryParam(queryParams, $"{clarifyGoFilters.PhoneNumber}", filters.PhoneNumber);
            AddQueryParam(queryParams, $"{clarifyGoFilters.Direction}", filters.CallDirection);
            AddQueryParam(queryParams, $"{clarifyGoFilters.Device}", filters.DeviceNumber);
            AddQueryParam(queryParams, $"{clarifyGoFilters.HuntGroupNumber}", filters.HuntGroupNumber);
            AddQueryParam(queryParams, $"{clarifyGoFilters.AccountCode}", filters.AccountCode);
            AddQueryParam(queryParams, $"{clarifyGoFilters.CallId}", filters.CallId);

            // Content filters
            AddQueryParam(queryParams, $"{clarifyGoFilters.CommentText}", filters.CommentContains);
            AddQueryParam(queryParams, $"{clarifyGoFilters.Tag}", filters.TagName);
            AddQueryParam(queryParams, $"{clarifyGoFilters.Bookmark}", filters.BookmarkText);

            // Recorder filters
            AddQueryParam(queryParams, $"{clarifyGoFilters.RedundancyType}", filters.RecorderType);
            AddQueryParam(queryParams, $"{clarifyGoFilters.RecorderFilter}", filters.RecorderId);

            // Sorting parameter
            AddQueryParam(queryParams, $"{clarifyGoFilters.SortBy}", filters.SortBy);

            // Advanced Filters
            AddQueryParam(queryParams, $"{clarifyGoFilters.RecordingGroupingId}", filters.RecordingGroupId);

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

        public async Task<PagedResponseDto<ClarifyGoHistoricRecordingRaw>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto, PaginationDto pagination)
        {
            try
            {
                await _tokenService.SetBearerTokenAsync(_httpClient);

                var baseUrl = ClarifyGoApiEndpoints.HistoricRecordings.Search(
                    searchFiltersDto.StartDate,
                    searchFiltersDto.EndDate);

                var queryParams = BuildQueryParameters(searchFiltersDto);

                var fullUrl = queryParams.Any()
                    ? $"{baseUrl}?{string.Join("&", queryParams)}"
                    : baseUrl;

                var response = await _httpClient.GetAsync(fullUrl);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ServiceException("Unauthorized access to recording service", 401);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new ServiceException($"Recording service error: {error}", (int)response.StatusCode);
                }

                var searchResultsObj = await response.Content.ReadFromJsonAsync<ClarifyGoHistoricRecordingSearchResults>();
                if (searchResultsObj == null)
                {
                    throw new ServiceException("Invalid response from recording service", 502);
                }

                var recordings = searchResultsObj.SearchResults?
                    .Select(x => x.HistoricRecording)
                    .ToList() ?? new List<ClarifyGoHistoricRecordingRaw>();

                var totalCount = searchResultsObj.TotalResults;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

                return new PagedResponseDto<ClarifyGoHistoricRecordingRaw>
                {
                    Items = recordings,
                    PageOffset = pagination.PageOffset,
                    PageSize = pagination.PageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    HasNext = (pagination.PageOffset + 1) * pagination.PageSize < totalCount,
                    HasPrevious = pagination.PageOffset > 0
                };
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

        public async Task<bool> DeleteRecordingAsync(string recordingId)
        {
            try
            {
                await _tokenService.SetBearerTokenAsync(_httpClient);

                var url = ClarifyGoApiEndpoints.HistoricRecordings.Delete(recordingId);

                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Failed to delete recording. Status code: {response.StatusCode}. Error: {error}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected error: {ex.Message}");
            }
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