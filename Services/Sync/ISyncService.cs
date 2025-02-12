using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services.Sync
{
    public interface ISyncService
    {
        Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate);

        Task SyncRecordingByIdAsync(string recordingId);
    }
}
