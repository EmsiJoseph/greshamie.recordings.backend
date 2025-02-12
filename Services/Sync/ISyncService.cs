using System;
using System.Threading;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.Sync
{
    public interface ISyncService
    {
        Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate);

        Task SyncRecordingByObjectAsync(RecordingDto dto);
    }
}
