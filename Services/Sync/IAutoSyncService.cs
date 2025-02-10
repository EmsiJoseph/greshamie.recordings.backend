using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services.Sync
{
    public interface IAutoSyncService : IHostedService, IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate);
        void Dispose();
    }
}
