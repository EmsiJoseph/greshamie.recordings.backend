using backend.Models;

namespace backend.Services.Comments;

public interface ICommentsService
{
    Task<IEnumerable<Comment>> GetCommentsAsync(string recordingId, bool isLiveRecording);
    Task AddCommentAsync(string recordingId, string comment, bool isLiveRecording);
    Task DeleteCommentAsync(string recordingId, string commentId, bool isLiveRecording);
}