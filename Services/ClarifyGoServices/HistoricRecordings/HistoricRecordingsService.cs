using System.Net;
using System.Text.Json;
using backend.ClarifyGoClasses;
using backend.Constants;
using backend.DTOs;
using backend.Exceptions;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using backend.Utilities;

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

        private string BuildQueryParameters(RecordingSearchFiltersDto? filters)
        {
            var builder = new QueryParameterBuilder();

            if (filters == null) return string.Empty;

            // Time of day filters
            if (filters.EarliestTimeOfDay.HasValue || filters.LatestTimeOfDay.HasValue)
            {
                builder
                    .AddParameter(ClarifyGoQueryParameters.MinTimeOfDay, filters.EarliestTimeOfDay)
                    .AddParameter(ClarifyGoQueryParameters.MaxTimeOfDay, filters.LatestTimeOfDay);
            }

            // Boolean parameters
            builder
                .AddParameter(ClarifyGoQueryParameters.HasScreenRecording, filters.HasScreenRecording)
                .AddParameter(ClarifyGoQueryParameters.HasPciEvents, filters.HasPciComplianceEvents)
                .AddParameter(ClarifyGoQueryParameters.HasRecordingEvaluation, filters.HasQualityEvaluation)
                .AddParameter(ClarifyGoQueryParameters.FilterRecordingByCompletionTime,
                    filters.FilterByRecordingEndTime)
                .AddParameter(ClarifyGoQueryParameters.SearchUnallocatedDevices, filters.SearchUnallocatedDevices)
                .AddParameter(ClarifyGoQueryParameters.SortDescending, filters.SortDescending);

            // Numeric parameters
            builder
                .AddParameter(ClarifyGoQueryParameters.MinDurationSeconds, filters.MinimumDurationSeconds)
                .AddParameter(ClarifyGoQueryParameters.MaxDurationSeconds, filters.MaximumDurationSeconds)
                .AddParameter(ClarifyGoQueryParameters.FirstResultIndex, filters.PageOffset)
                .AddParameter(ClarifyGoQueryParameters.MaxResults, filters.PageSize);

            // String parameters
            builder
                .AddParameter(ClarifyGoQueryParameters.PhoneNumber, filters.PhoneNumber)
                .AddParameter(ClarifyGoQueryParameters.Direction, filters.CallDirection.ToLowerInvariant())
                .AddParameter(ClarifyGoQueryParameters.Device, filters.DeviceNumber)
                .AddParameter(ClarifyGoQueryParameters.HuntGroupNumber, filters.HuntGroupNumber)
                .AddParameter(ClarifyGoQueryParameters.AccountCode, filters.AccountCode)
                .AddParameter(ClarifyGoQueryParameters.CallId, filters.CallId)
                .AddParameter(ClarifyGoQueryParameters.CommentText, filters.CommentContains)
                .AddParameter(ClarifyGoQueryParameters.Tag, filters.TagName)
                .AddParameter(ClarifyGoQueryParameters.Bookmark, filters.BookmarkText)
                .AddParameter(ClarifyGoQueryParameters.RedundancyType, filters.RecorderType)
                .AddParameter(ClarifyGoQueryParameters.RecorderFilter, filters.RecorderId)
                .AddParameter(ClarifyGoQueryParameters.SortBy, filters.SortBy)
                .AddParameter(ClarifyGoQueryParameters.RecordingGroupingId, filters.RecordingGroupId);

            return builder.Build();
        }

        public async Task<PagedResponseDto<ClarifyGoHistoricRecordingRaw>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto, PaginationDto? pagination)
        {
            try
            {
                await _tokenService.SetBearerTokenAsync(_httpClient);

                var baseUrl = ClarifyGoApiEndpoints.HistoricRecordings.Search(
                    searchFiltersDto.StartDate,
                    searchFiltersDto.EndDate);

                var queryString = BuildQueryParameters(searchFiltersDto);
                var fullUrl = baseUrl + queryString;
                
                Console.WriteLine($"Full URL: {fullUrl}");

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

                var searchResultsObj =
                    await response.Content.ReadFromJsonAsync<ClarifyGoHistoricRecordingSearchResults>();
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