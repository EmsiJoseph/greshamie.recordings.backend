using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Classes;
using backend.DTOs;
using backend.Models;

namespace backend.Services.ClarifyGoServices.HistoricRecordings
{
    public interface IHistoricRecordingsService
    {
        /// <summary>
        /// Searches for historic recordings based on the specified date range and filter criteria.
        /// </summary>
        /// <param name="startDate">The start date for the search period.</param>
        /// <param name="endDate">The end date for the search period.</param>
        /// <param name="searchFiltersDto">Optional frontend-friendly filter criteria.</param>
        /// <returns>A <see cref="HistoricRecordingSearchResults"/> object containing the search results.</returns>
        Task<IEnumerable<HistoricRecordingSearchResult>> SearchRecordingsAsync(
            RecordingSearchFiltersDto searchFiltersDto);

        /// <summary>
        /// Deletes a recording specified by its ID.
        /// </summary>
        Task DeleteRecordingAsync(string recordingId);

        /// <summary>
        /// Exports the specified recording as an MP3.
        /// </summary>
        Task<Stream> ExportMp3Async(string recordingId);

        /// <summary>
        /// Exports the specified recording as a WAV.
        /// </summary>
        Task<Stream> ExportWavAsync(string recordingId);
    }
}