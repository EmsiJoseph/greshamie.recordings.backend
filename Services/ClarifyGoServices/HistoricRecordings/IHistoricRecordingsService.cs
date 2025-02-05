using backend.Classes;

namespace backend.Services.ClarifyGoServices.HistoricRecordings;

public interface IHistoricRecordingsService
{
    Task<RecordingSearchResults> SearchRecordingsAsync(DateTime start, DateTime end);
    Task DeleteRecordingAsync(string recordingId);
    Task<Stream> ExportMp3Async(string recordingId);
    Task<Stream> ExportWavAsync(string recordingId);
}