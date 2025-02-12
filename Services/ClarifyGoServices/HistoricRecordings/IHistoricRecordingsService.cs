using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using backend.ClarifyGoClasses;
using backend.DTOs;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public interface IHistoricRecordingsService
    {
        /// <summary>
        /// Searches for historic recordings with pagination support.
        /// </summary>
        /// <param name="searchFiltersDto">Search and filter criteria.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <returns>A paged response containing the search results.</returns>
        Task<PagedResponseDto<ClarifyGoHistoricRecordingRaw>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto, PaginationDto? pagination = null);

        /// <summary>
        /// Deletes a recording specified by its ID.
        /// </summary>
        Task<bool> DeleteRecordingAsync(string recordingId);

        /// <summary>
        /// Exports the specified recording as an MP3.
        /// Player Command
        /// </summary>
        Task<Stream> ExportMp3Async(string recordingId);

        /// <summary>
        /// Exports the specified recording as a WAV.
        /// </summary>
        Task<Stream> ExportWavAsync(string recordingId);

        /// <summary>
        /// Sets the bearer token for authentication.
        /// </summary>
        void SetBearerToken(string token);
    }
}