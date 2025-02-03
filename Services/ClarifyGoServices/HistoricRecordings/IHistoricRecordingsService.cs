namespace backend.Services.LiveRecordings;

public interface ILiveRecordingsService
{
    Task<IEnumerable<LiveRecording>> GetLiveRecordingsAsync();
    Task ResumeRecordingAsync(string recorderId, string recordingId);
    Task PauseRecordingAsync(string recorderId, string recordingId);
}