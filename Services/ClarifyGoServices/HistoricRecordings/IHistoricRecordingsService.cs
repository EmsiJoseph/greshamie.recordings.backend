using backend.Classes;
using backend.Models;

namespace backend.Services.LiveRecordings;

public interface IHistoricRecordingsService
{
    Task<RecordingSearchResults> SearchRecordingsAsync(DateTime start, DateTime end);
    Task DeleteRecordingAsync(string recordingId);
    Task<Stream> ExportMp3Async(string recordingId);
    Task<Stream> ExportWavAsync(string recordingId);
}