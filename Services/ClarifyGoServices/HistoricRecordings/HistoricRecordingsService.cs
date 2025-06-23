using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using backend.ClarifyGoClasses;
using backend.Constants.ClarifyGo;
using backend.Data.Models;
using backend.DTOs;
using backend.DTOs.Recording;
using backend.Exceptions;
using backend.Services.Auth;
using IdentityModel.Client;
using backend.Utilities;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public class HistoricRecordingsService : IHistoricRecordingsService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILogger<HistoricRecordingsService> _logger;

        public HistoricRecordingsService(HttpClient httpClient, ITokenService tokenService, ILogger<HistoricRecordingsService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("Initialized HistoricRecordingsService");
        }

        public void SetBearerToken(string token)
        {
            try
            {
                _httpClient.SetBearerToken(token);
                _logger.LogInformation("Successfully set bearer token for HTTP client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set bearer token for HTTP client");
                throw;
            }
        }

        private string BuildQueryParameters(RecordingSearchFiltersDto? filters)
        {
            var builder = new QueryParameterBuilder();

            if (filters == null)
            {
                _logger.LogWarning("BuildQueryParameters called with null filters, returning empty query string");
                return string.Empty;
            }

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
                .AddParameter(ClarifyGoQueryParameters.FilterRecordingByCompletionTime, filters.FilterByRecordingEndTime)
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
                .AddParameter(ClarifyGoQueryParameters.Direction, filters.CallDirection?.ToLowerInvariant())
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

            var queryString = builder.Build();
            _logger.LogDebug("Built query parameters for search: {QueryString}", queryString);
            return queryString;
        }

        public async Task<PagedResponseDto<HistoricRecordingRaw>> SearchRecordingsAsync(RecordingSearchFiltersDto searchFiltersDto)
        {
            try
            {
                _logger.LogInformation("Starting search for raw recordings: StartDate={StartDate}, EndDate={EndDate}, RecorderId={RecorderId}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate, searchFiltersDto.RecorderId ?? "null");

                // First call to get current page and total pages
                try
                {
                    await _tokenService.SetBearerTokenAsync(_httpClient);
                    _logger.LogDebug("Set bearer token for search request");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set bearer token for search request");
                    throw;
                }

                var baseUrl = ClarifyGoApiEndpoints.HistoricRecordings.Search(searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                var queryString = BuildQueryParameters(searchFiltersDto);
                var requestUrl = baseUrl + queryString;

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.GetAsync(requestUrl);
                    _logger.LogDebug("Sent GET request to {RequestUrl}", requestUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send GET request to {RequestUrl}", requestUrl);
                    throw;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to recording service at {RequestUrl}, StatusCode={StatusCode}", 
                        requestUrl, response.StatusCode);
                    throw new ServiceException("Unauthorized access to recording service", 401);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Recording service error at {RequestUrl}, StatusCode={StatusCode}, Error={Error}", 
                        requestUrl, response.StatusCode, error);
                    throw new ServiceException($"Recording service error: {error}", (int)response.StatusCode);
                }

                HistoricRecordingSearchResults? searchResultsObj;
                try
                {
                    searchResultsObj = await response.Content.ReadFromJsonAsync<HistoricRecordingSearchResults>();
                    if (searchResultsObj == null)
                    {
                        _logger.LogError("Invalid null response from recording service at {RequestUrl}", requestUrl);
                        throw new ServiceException("Invalid response from recording service", 502);
                    }
                    _logger.LogInformation("Successfully parsed {Count} search results from {RequestUrl}", 
                        searchResultsObj.SearchResults.Count, requestUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse JSON response from {RequestUrl}", requestUrl);
                    throw;
                }

                var recordings = searchResultsObj.SearchResults.Select(x => x.HistoricRecording).ToList();
                var totalPages = searchResultsObj.TotalResults;

                if (!recordings.Any())
                {
                    _logger.LogInformation("No recordings found for filters: StartDate={StartDate}, EndDate={EndDate}, RecorderId={RecorderId}", 
                        searchFiltersDto.StartDate, searchFiltersDto.EndDate, searchFiltersDto.RecorderId ?? "null");
                }

                // Second call to get last page
                if (totalPages > 0)
                {
                    var lastPageFilters = new RecordingSearchFiltersDto
                    {
                        StartDate = searchFiltersDto.StartDate,
                        EndDate = searchFiltersDto.EndDate,
                        EarliestTimeOfDay = searchFiltersDto.EarliestTimeOfDay,
                        LatestTimeOfDay = searchFiltersDto.LatestTimeOfDay,
                        HasScreenRecording = searchFiltersDto.HasScreenRecording,
                        HasPciComplianceEvents = searchFiltersDto.HasPciComplianceEvents,
                        HasQualityEvaluation = searchFiltersDto.HasQualityEvaluation,
                        FilterByRecordingEndTime = searchFiltersDto.FilterByRecordingEndTime,
                        SearchUnallocatedDevices = searchFiltersDto.SearchUnallocatedDevices,
                        SortDescending = searchFiltersDto.SortDescending,
                        MinimumDurationSeconds = searchFiltersDto.MinimumDurationSeconds,
                        MaximumDurationSeconds = searchFiltersDto.MaximumDurationSeconds,
                        PageOffset = totalPages - 1,
                        PageSize = searchFiltersDto.PageSize,
                        PhoneNumber = searchFiltersDto.PhoneNumber,
                        CallDirection = searchFiltersDto.CallDirection,
                        DeviceNumber = searchFiltersDto.DeviceNumber,
                        HuntGroupNumber = searchFiltersDto.HuntGroupNumber,
                        AccountCode = searchFiltersDto.AccountCode,
                        CallId = searchFiltersDto.CallId,
                        CommentContains = searchFiltersDto.CommentContains,
                        TagName = searchFiltersDto.TagName,
                        BookmarkText = searchFiltersDto.BookmarkText,
                        RecorderType = searchFiltersDto.RecorderType,
                        RecorderId = searchFiltersDto.RecorderId,
                        SortBy = searchFiltersDto.SortBy,
                        RecordingGroupId = searchFiltersDto.RecordingGroupId
                    };

                    var lastPageQueryString = BuildQueryParameters(lastPageFilters);
                    var lastPageRequestUrl = baseUrl + lastPageQueryString;

                    HttpResponseMessage lastPageResponse;
                    try
                    {
                        lastPageResponse = await _httpClient.GetAsync(lastPageRequestUrl);
                        _logger.LogDebug("Sent GET request for last page to {LastPageRequestUrl}", lastPageRequestUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send GET request for last page to {LastPageRequestUrl}", lastPageRequestUrl);
                        throw;
                    }

                    if (lastPageResponse.IsSuccessStatusCode)
                    {
                        HistoricRecordingSearchResults? lastPageResults;
                        try
                        {
                            lastPageResults = await lastPageResponse.Content.ReadFromJsonAsync<HistoricRecordingSearchResults>();
                            if (lastPageResults != null)
                            {
                                var lastPageCount = lastPageResults.SearchResults.Count;
                                var fullPagesCount = (totalPages - 1) * (searchFiltersDto.PageSize ?? 0);
                                var totalCount = fullPagesCount + lastPageCount;

                                _logger.LogInformation("Completed search with {TotalCount} total raw recordings, TotalPages={TotalPages}", 
                                    totalCount, totalPages);

                                return new PagedResponseDto<HistoricRecordingRaw>
                                {
                                    Items = recordings,
                                    PageOffSet = searchFiltersDto.PageOffset,
                                    PageSize = searchFiltersDto.PageSize,
                                    TotalPages = totalPages,
                                    TotalCount = totalCount,
                                    HasNext = searchFiltersDto.PageOffset < totalPages - 1,
                                    HasPrevious = searchFiltersDto.PageOffset > 0
                                };
                            }
                            _logger.LogWarning("Invalid null last page response from {LastPageRequestUrl}", lastPageRequestUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse last page JSON response from {LastPageRequestUrl}", lastPageRequestUrl);
                            throw;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Last page request failed at {LastPageRequestUrl}, StatusCode={StatusCode}", 
                            lastPageRequestUrl, lastPageResponse.StatusCode);
                    }
                }

                // Fallback if last page request fails
                var estimatedTotalCount = totalPages * (searchFiltersDto.PageSize ?? 0);
                _logger.LogInformation("Completed search with estimated {TotalCount} total raw recordings, TotalPages={TotalPages}", 
                    estimatedTotalCount, totalPages);

                return new PagedResponseDto<HistoricRecordingRaw>
                {
                    Items = recordings,
                    PageOffSet = searchFiltersDto.PageOffset,
                    PageSize = searchFiltersDto.PageSize,
                    TotalPages = totalPages,
                    TotalCount = estimatedTotalCount,
                    HasNext = searchFiltersDto.PageOffset < totalPages - 1,
                    HasPrevious = searchFiltersDto.PageOffset > 0
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during raw recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw new ServiceException($"Network error: {ex.Message}", 503);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid response format during raw recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw new ServiceException($"Invalid response format: {ex.Message}", 502);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Service error during raw recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during raw recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw new ServiceException($"Unexpected error: {ex.Message}");
            }
        }
        
        public async Task<PagedResponseDto<HistoricRecordingSearchResult>> SearchProcessedRecordingsAsync(RecordingSearchFiltersDto searchFiltersDto)
        {
            try
            {
                _logger.LogInformation("Starting search for processed recordings: StartDate={StartDate}, EndDate={EndDate}, RecorderId={RecorderId}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate, searchFiltersDto.RecorderId ?? "null");

                try
                {
                    await _tokenService.SetBearerTokenAsync(_httpClient);
                    _logger.LogDebug("Set bearer token for processed recordings search request");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set bearer token for processed recordings search request");
                    throw;
                }

                var baseUrl = ClarifyGoApiEndpoints.HistoricRecordings.Search(searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                var queryString = BuildQueryParameters(searchFiltersDto);
                var requestUrl = baseUrl + queryString;

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.GetAsync(requestUrl);
                    _logger.LogDebug("Sent GET request to {RequestUrl}", requestUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send GET request to {RequestUrl}", requestUrl);
                    throw;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to recording service at {RequestUrl}, StatusCode={StatusCode}", 
                        requestUrl, response.StatusCode);
                    throw new ServiceException("Unauthorized access to recording service", 401);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Recording service error at {RequestUrl}, StatusCode={StatusCode}, Error={Error}", 
                        requestUrl, response.StatusCode, error);
                    throw new ServiceException($"Recording service error: {error}", (int)response.StatusCode);
                }

                HistoricRecordingSearchResults? searchResultsObj;
                try
                {
                    searchResultsObj = await response.Content.ReadFromJsonAsync<HistoricRecordingSearchResults>();
                    if (searchResultsObj == null)
                    {
                        _logger.LogError("Invalid null response from recording service at {RequestUrl}", requestUrl);
                        throw new ServiceException("Invalid response from recording service", 502);
                    }
                    _logger.LogInformation("Successfully parsed {Count} search results from {RequestUrl}", 
                        searchResultsObj.SearchResults.Count, requestUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse JSON response from {RequestUrl}", requestUrl);
                    throw;
                }

                if (!searchResultsObj.SearchResults.Any())
                {
                    _logger.LogInformation("No processed recordings found for filters: StartDate={StartDate}, EndDate={EndDate}, RecorderId={RecorderId}", 
                        searchFiltersDto.StartDate, searchFiltersDto.EndDate, searchFiltersDto.RecorderId ?? "null");
                }

                // Process recordings with regex
                _logger.LogDebug("Starting regex processing for {Count} search results", searchResultsObj.SearchResults.Count);
                var parenthesesRegex = new Regex(@"\(([^)]+)\)", RegexOptions.IgnoreCase);
                var numberWithNameRegex = new Regex(@"^(.+?)\s*\(([^)]*)\)$", RegexOptions.IgnoreCase);
                var meetingRegex = new Regex(@"^Meeting\s*\(([^)]*)\)$", RegexOptions.IgnoreCase);
                var numberOnlyRegex = new Regex(@"^\+\d+$");

                var processedResults = searchResultsObj.SearchResults.Select(result =>
                {
                    var rawRecording = result.HistoricRecording;
                    string callingParty = rawRecording.CallingParty ?? string.Empty;
                    string? callingPartyNumber = null;

                    try
                    {
                        if (string.IsNullOrEmpty(callingParty))
                        {
                            callingParty = "Unknown";
                        }
                        else
                        {
                            var numberMatch = numberWithNameRegex.Match(callingParty);
                            if (numberMatch.Success)
                            {
                                callingPartyNumber = numberMatch.Groups[1].Value.Trim();
                                callingParty = numberMatch.Groups[2].Value.Trim();
                                if (string.IsNullOrEmpty(callingParty))
                                {
                                    callingParty = "Unknown";
                                }
                            }
                            else if (numberOnlyRegex.IsMatch(callingParty))
                            {
                                callingPartyNumber = callingParty;
                                callingParty = callingParty;
                            }
                            else
                            {
                                var parenthesesMatch = parenthesesRegex.Match(callingParty);
                                if (parenthesesMatch.Success)
                                {
                                    callingParty = parenthesesMatch.Groups[1].Value.Trim();
                                }
                            }
                        }

                        string calledParty = rawRecording.CalledParty ?? string.Empty;
                        string? calledPartyNumber = null;
                        var meetingMatch = meetingRegex.Match(calledParty);
                        if (meetingMatch.Success)
                        {
                            string name = meetingMatch.Groups[1].Value.Trim();
                            calledParty = string.IsNullOrEmpty(name) ? "Meeting (Unknown)" : $"Meeting ({name})";
                        }
                        else
                        {
                            var numberMatch = numberWithNameRegex.Match(calledParty);
                            if (numberMatch.Success)
                            {
                                calledPartyNumber = numberMatch.Groups[1].Value.Trim();
                                calledParty = numberMatch.Groups[2].Value.Trim();
                                if (string.IsNullOrEmpty(calledParty))
                                {
                                    calledParty = "Unknown";
                                }
                            }
                            else if (numberOnlyRegex.IsMatch(calledParty))
                            {
                                calledPartyNumber = calledParty;
                                calledParty = callingParty;
                            }
                            else if (calledParty.StartsWith("Meeting", StringComparison.OrdinalIgnoreCase))
                            {
                                calledParty = "Meeting (Unknown)";
                            }
                            else
                            {
                                var parenthesesMatch = parenthesesRegex.Match(calledParty);
                                if (parenthesesMatch.Success)
                                {
                                    calledParty = parenthesesMatch.Groups[1].Value.Trim();
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(rawRecording.CallingParty) && rawRecording.CallingParty == rawRecording.CalledParty)
                        {
                            calledPartyNumber = callingPartyNumber;
                            calledParty = callingParty;
                        }

                        var processedRecording = new ProcessedRecording
                        {
                            AccountId = rawRecording.AccountId,
                            PbxId = rawRecording.PbxId,
                            PbxAccountEndpoints = rawRecording.PbxAccountEndpoints,
                            MediaCompletedTime = rawRecording.MediaCompletedTime,
                            AlertedTime = rawRecording.AlertedTime,
                            ConnectedTime = rawRecording.ConnectedTime,
                            DisconnectedTime = rawRecording.DisconnectedTime,
                            MediaServerId = rawRecording.MediaServerId,
                            RecorderClusterId = rawRecording.RecorderClusterId,
                            RecordingGroupingId = rawRecording.RecordingGroupingId,
                            DirectRecordingLink = rawRecording.DirectRecordingLink,
                            Id = rawRecording.Id,
                            RecorderId = rawRecording.RecorderId,
                            PbxAccounts = rawRecording.PbxAccounts,
                            CallType = rawRecording.CallType,
                            CallingParty = callingParty,
                            CalledParty = calledParty,
                            State = rawRecording.State,
                            Channel = rawRecording.Channel,
                            MediaStartedTime = rawRecording.MediaStartedTime,
                            IsHidden = rawRecording.IsHidden,
                            CallingPartyNumber = callingPartyNumber,
                            CalledPartyNumber = calledPartyNumber
                        };

                        _logger.LogDebug("Processed recording {RecordingId}: CallingParty={CallingParty}, CalledParty={CalledParty}", 
                            rawRecording.Id ?? "null", callingParty, calledParty);

                        return new HistoricRecordingSearchResult
                        {
                            HistoricRecording = processedRecording,
                            ScreenRecordingCount = result.ScreenRecordingCount,
                            TagCount = result.TagCount,
                            CommentCount = result.CommentCount,
                            PciEventCount = result.PciEventCount,
                            RecordingEvaluationCount = result.RecordingEvaluationCount
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process recording {RecordingId}", rawRecording.Id ?? "null");
                        throw;
                    }
                }).ToList();

                _logger.LogInformation("Completed regex processing for {Count} processed recordings", processedResults.Count);

                var totalPages = searchResultsObj.TotalResults;

                if (totalPages > 0)
                {
                    var lastPageFilters = new RecordingSearchFiltersDto
                    {
                        StartDate = searchFiltersDto.StartDate,
                        EndDate = searchFiltersDto.EndDate,
                        EarliestTimeOfDay = searchFiltersDto.EarliestTimeOfDay,
                        LatestTimeOfDay = searchFiltersDto.LatestTimeOfDay,
                        HasScreenRecording = searchFiltersDto.HasScreenRecording,
                        HasPciComplianceEvents = searchFiltersDto.HasPciComplianceEvents,
                        HasQualityEvaluation = searchFiltersDto.HasQualityEvaluation,
                        FilterByRecordingEndTime = searchFiltersDto.FilterByRecordingEndTime,
                        SearchUnallocatedDevices = searchFiltersDto.SearchUnallocatedDevices,
                        SortDescending = searchFiltersDto.SortDescending,
                        MinimumDurationSeconds = searchFiltersDto.MinimumDurationSeconds,
                        MaximumDurationSeconds = searchFiltersDto.MaximumDurationSeconds,
                        PageOffset = totalPages - 1,
                        PageSize = searchFiltersDto.PageSize,
                        PhoneNumber = searchFiltersDto.PhoneNumber,
                        CallDirection = searchFiltersDto.CallDirection,
                        DeviceNumber = searchFiltersDto.DeviceNumber,
                        HuntGroupNumber = searchFiltersDto.HuntGroupNumber,
                        AccountCode = searchFiltersDto.AccountCode,
                        CallId = searchFiltersDto.CallId,
                        CommentContains = searchFiltersDto.CommentContains,
                        TagName = searchFiltersDto.TagName,
                        BookmarkText = searchFiltersDto.BookmarkText,
                        RecorderType = searchFiltersDto.RecorderType,
                        RecorderId = searchFiltersDto.RecorderId,
                        SortBy = searchFiltersDto.SortBy,
                        RecordingGroupId = searchFiltersDto.RecordingGroupId
                    };

                    var lastPageQueryString = BuildQueryParameters(lastPageFilters);
                    var lastPageRequestUrl = baseUrl + lastPageQueryString;

                    HttpResponseMessage lastPageResponse;
                    try
                    {
                        lastPageResponse = await _httpClient.GetAsync(lastPageRequestUrl);
                        _logger.LogDebug("Sent GET request for last page to {LastPageRequestUrl}", lastPageRequestUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send GET request for last page to {LastPageRequestUrl}", lastPageRequestUrl);
                        throw;
                    }

                    if (lastPageResponse.IsSuccessStatusCode)
                    {
                        HistoricRecordingSearchResults? lastPageResults;
                        try
                        {
                            lastPageResults = await lastPageResponse.Content.ReadFromJsonAsync<HistoricRecordingSearchResults>();
                            if (lastPageResults != null)
                            {
                                var lastPageCount = lastPageResults.SearchResults.Count;
                                var fullPagesCount = (totalPages - 1) * (searchFiltersDto.PageSize ?? 0);
                                var totalCount = fullPagesCount + lastPageCount;

                                _logger.LogInformation("Completed search with {TotalCount} total processed recordings, TotalPages={TotalPages}", 
                                    totalCount, totalPages);

                                return new PagedResponseDto<HistoricRecordingSearchResult>
                                {
                                    Items = processedResults,
                                    PageOffSet = searchFiltersDto.PageOffset,
                                    PageSize = searchFiltersDto.PageSize,
                                    TotalPages = totalPages,
                                    TotalCount = totalCount,
                                    HasNext = searchFiltersDto.PageOffset < totalPages - 1,
                                    HasPrevious = searchFiltersDto.PageOffset > 0
                                };
                            }
                            _logger.LogWarning("Invalid null last page response from {LastPageRequestUrl}", lastPageRequestUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse last page JSON response from {LastPageRequestUrl}", lastPageRequestUrl);
                            throw;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Last page request failed at {LastPageRequestUrl}, StatusCode={StatusCode}", 
                            lastPageRequestUrl, lastPageResponse.StatusCode);
                    }
                }

                var estimatedTotalCount = totalPages * (searchFiltersDto.PageSize ?? 0);
                _logger.LogInformation("Completed search with estimated {TotalCount} total processed recordings, TotalPages={TotalPages}", 
                    estimatedTotalCount, totalPages);

                return new PagedResponseDto<HistoricRecordingSearchResult>
                {
                    Items = processedResults,
                    PageOffSet = searchFiltersDto.PageOffset,
                    PageSize = searchFiltersDto.PageSize,
                    TotalPages = totalPages,
                    TotalCount = estimatedTotalCount,
                    HasNext = searchFiltersDto.PageOffset < totalPages - 1,
                    HasPrevious = searchFiltersDto.PageOffset > 0
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during processed recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw new ServiceException($"Network error: {ex.Message}", 503);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid response format during processed recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw new ServiceException($"Invalid response format: {ex.Message}", 502);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Service error during processed recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during processed recordings search: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFiltersDto.StartDate, searchFiltersDto.EndDate);
                throw new ServiceException($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteRecordingAsync(string recordingId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete recording {RecordingId}", recordingId);

                try
                {
                    await _tokenService.SetBearerTokenAsync(_httpClient);
                    _logger.LogDebug("Set bearer token for delete request");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set bearer token for delete request");
                    throw;
                }

                var url = ClarifyGoApiEndpoints.HistoricRecordings.Delete(recordingId);

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.DeleteAsync(url);
                    _logger.LogDebug("Sent DELETE request to {RequestUrl}", url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send DELETE request to {RequestUrl}", url);
                    throw;
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted recording {RecordingId}", recordingId);
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to delete recording {RecordingId}, StatusCode={StatusCode}, Error={Error}", 
                        recordingId, response.StatusCode, error);
                    throw new HttpRequestException(
                        $"Failed to delete recording. Status code: {response.StatusCode}. Error: {error}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while deleting recording {RecordingId}", recordingId);
                throw new ServiceException($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting recording {RecordingId}", recordingId);
                throw new ServiceException($"Unexpected error: {ex.Message}");
            }
        }

       public async Task<Stream> ExportMp3Async(string recordingId)
        {
            try
            {
                _logger.LogInformation("Exporting MP3 for recording {RecordingId}", recordingId);

                try
                {
                    await _tokenService.SetBearerTokenAsync(_httpClient);
                    _logger.LogDebug("Set bearer token for MP3 export request");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set bearer token for MP3 export request");
                    throw;
                }

                var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportMp3(recordingId);

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    _logger.LogDebug("Sent GET request for MP3 export to {RequestUrl}", url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send GET request for MP3 export to {RequestUrl}", url);
                    throw;
                }

                try
                {
                    response.EnsureSuccessStatusCode();
                    var stream = await response.Content.ReadAsStreamAsync();
                    _logger.LogInformation("Successfully exported MP3 stream for recording {RecordingId}", recordingId);
                    return stream;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve MP3 stream for recording {RecordingId}, StatusCode={StatusCode}", 
                        recordingId, response.StatusCode);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while exporting MP3 for recording {RecordingId}", recordingId);
                throw;
            }
        }

        public async Task<Stream> ExportWavAsync(string recordingId)
        {
            try
            {
                _logger.LogInformation("Exporting WAV for recording {RecordingId}", recordingId);

                try
                {
                    await _tokenService.SetBearerTokenAsync(_httpClient);
                    _logger.LogDebug("Set bearer token for WAV export request");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set bearer token for WAV export request");
                    throw;
                }

                var url = ClarifyGoApiEndpoints.HistoricRecordings.ExportWav(recordingId);

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    _logger.LogDebug("Sent GET request for WAV export to {RequestUrl}", url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send GET request for WAV export to {RequestUrl}", url);
                    throw;
                }

                try
                {
                    response.EnsureSuccessStatusCode();
                    var stream = await response.Content.ReadAsStreamAsync();
                    _logger.LogInformation("Successfully exported WAV stream for recording {RecordingId}", recordingId);
                    return stream;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve WAV stream for recording {RecordingId}, StatusCode={StatusCode}", 
                        recordingId, response.StatusCode);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while exporting WAV for recording {RecordingId}", recordingId);
                throw;
            }
        }
    }
}