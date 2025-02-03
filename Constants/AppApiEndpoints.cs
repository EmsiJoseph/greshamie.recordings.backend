namespace backend.Constants;

public static class AppApiEndpoints
{
    public static class HistoricRecordings
    {
        private const string Base = "/api/recordings/historic";
        public const string Search = $"{Base}/search/{{startDate}}/{{endDate}}";
        public const string Delete = $"{Base}/{{recordingId}}";
        public const string ExportMp3 = $"{Base}/{{recordingId}}/mp3";
        public const string ExportWav = $"{Base}/{{recordingId}}/wav";
        
        public const string GetPostComments = $"{Base}/{{recordingId}}/comments";
        public const string DeleteComment = $"{Base}/{{recordingId}}/comments/{{commentId}}";
        
        public const string GetTags = $"{Base}/{{recordingId}}/tags";
        public const string PostDeleteTag = $"{Base}/{{recordingId}}/tags/{{tag}}";
    }

    public static class LiveRecordings
    {
        private const string Base = "/api/recordings/live";
        public const string GetAll = Base;
        public const string Resume = $"{Base}/{{recorderId}}/{{recordingId}}/resume";
        public const string Pause = $"{Base}/{{recorderId}}/{{recordingId}}/pause";
        
        public const string GetPostComments = $"{Base}/{{recordingId}}/comments";
        public const string DeleteComment = $"{Base}/{{recordingId}}/comments/{{commentId}}";
        
        public const string GetTags = $"{Base}/{{recordingId}}/tags";
        public const string PostDeleteTag = $"{Base}/{{recordingId}}/tags/{{tag}}";
    }

    public static class Tags
    {
        private const string Base = "/api/tags";
        public const string MostUsed = $"{Base}/mostused/{{limit}}";
    }
}