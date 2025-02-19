using backend.DTOs.Recording;

namespace backend.Services.Sync;

public interface ISyncService
{
    Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate);

    Task SyncRecordingByObjectAsync(RecordingDto dto);
}