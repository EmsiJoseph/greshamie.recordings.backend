using backend.DTOs;
using backend.Models;

namespace backend.Services.ClarifyGoServices.LiveRecordings;

public interface ILiveRecordingsService
{
    Task<IEnumerable<RecordingDto>> GetLiveRecordingsAsync();
    Task ResumeRecordingAsync(string recorderId, string recordingId);
    Task PauseRecordingAsync(string recorderId, string recordingId);
}