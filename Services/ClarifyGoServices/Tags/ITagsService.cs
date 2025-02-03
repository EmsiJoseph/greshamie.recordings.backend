using backend.Models;

namespace backend.Services.Tags;

public interface ITagsService
{
    Task<IEnumerable<Tag>> GetTagsAsync(string recordingId, bool isLiveRecording);
    Task AddTagAsync(string recordingId, string tag, bool isLiveRecording);
    Task RemoveTagAsync(string recordingId, string tag, bool isLiveRecording);
    Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int limit);
}