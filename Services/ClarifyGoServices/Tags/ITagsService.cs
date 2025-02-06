using backend.Classes;

namespace backend.Services.ClarifyGoServices.Tags;


public interface ITagsService
{
    Task<IEnumerable<Tag>> GetTagsAsync(string recordingId, bool isLiveRecording = false);
    Task PostTagAsync(string recordingId, string tag, bool isLiveRecording = false);
    Task DeleteTagAsync(string recordingId, string tag, bool isLiveRecording = false);
    Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int limit);
}