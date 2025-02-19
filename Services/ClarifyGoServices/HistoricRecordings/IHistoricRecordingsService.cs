using backend.ClarifyGoClasses;
using backend.DTOs;
using backend.DTOs.Recording;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public interface IHistoricRecordingsService
    {
        /// <summary>
        /// Searches for recordings in ClarifyGo.
        /// Returns a paginated list that can be filtered by various criteria.
        /// </summary>
        Task<PagedResponseDto<HistoricRecordingRaw>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto);

        /// <summary>
        /// Permanently deletes a recording from ClarifyGo.
        /// This action cannot be undone.
        /// </summary>
        Task<bool> DeleteRecordingAsync(string recordingId);

        /// <summary>
        /// Downloads a recording as an MP3 file.
        /// Best for general playback and sharing.
        /// </summary>
        Task<Stream> ExportMp3Async(string recordingId);

        /// <summary>
        /// Downloads a recording as a WAV file.
        /// Best for high quality audio analysis.
        /// </summary>
        Task<Stream> ExportWavAsync(string recordingId);

        /// <summary>
        /// Updates the authentication token for ClarifyGo API calls.
        /// Call this when getting new credentials.
        /// </summary>
        void SetBearerToken(string token);
    }
}