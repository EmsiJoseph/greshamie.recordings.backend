using backend.Classes;

namespace backend.Services.ClarifyGoServices.Comments;

public interface ICommentsService
{
    Task<IEnumerable<Comment>> GetCommentsAsync(string recordingId, bool isLiveRecording = false);
    Task PostCommentAsync(string recordingId, string comment, bool isLiveRecording = false);
    Task DeleteCommentAsync(string recordingId, string commentId, bool isLiveRecording = false);
}