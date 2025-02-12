using backend.ClarifyGoClasses;
using backend.DTOs;
using backend.DTOs.Recording;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public interface IHistoricRecordingsService
    {
        /// <summary>
        /// Searches for historic recordings with pagination support.
        /// </summary>
        /// <param name="searchFiltersDto">Search and filter criteria.</param>
        /// <returns>A paged response containing the search results.</returns>
        Task<PagedResponseDto<HistoricRecordingRaw>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto);

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