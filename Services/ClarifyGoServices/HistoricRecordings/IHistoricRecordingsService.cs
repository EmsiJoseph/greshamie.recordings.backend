using backend.Classes;

namespace backend.Services.ClarifyGoServices.HistoricRecordings;

public interface IHistoricRecordingsService
{
    Task<RecordingSearchResults> GetSearchRecordingsAsync(DateTime start, DateTime end);
    Task DeleteRecordingAsync(string recordingId);
    Task<Stream> GetExportMp3Async(string recordingId);
    Task<Stream> GetExportWavAsync(string recordingId);
}