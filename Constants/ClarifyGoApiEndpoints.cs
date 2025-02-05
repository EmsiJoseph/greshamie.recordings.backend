namespace backend.Constants;

public static class ClarifyGoApiEndpoints
{
    private const string ApiVersion = "v1.0";

    public static class HistoricRecordings
    {
        private const string Base = $"{ApiVersion}/historicrecordings";

        public const string GetSearch = $"{Base}/{{startDate}}/{{endDate}}";
        public const string Delete = $"{Base}/{{recordingId}}";
        public const string GetExportMp3 = $"{Base}/{{recordingId}}/Mp3";
        public const string GetExportWav = $"{Base}/{{recordingId}}/Wav";

        public const string GetPostComments = $"{Base}/{{recordingId}}/comments";
        public const string DeleteComment = $"{Base}/{{recordingId}}/comments/{{commentId}}";

        public const string GetTags = $"{Base}/{{recordingId}}/tags";
        public const string PostDeleteTag = $"{Base}/{{recordingId}}/tags/{{tag}}";
    }

    public static class LiveRecordings
    {
        private const string Base = $"{ApiVersion}/liverecordings";

        public const string GetAll = Base;
        public const string PutResume = $"{Base}/{{recorderId}}/{{recordingId}}/resume";
        public const string PutPause = $"{Base}/{{recorderId}}/{{recordingId}}/pause";

        public const string GetPostComments = $"{Base}/{{recordingId}}/comments";
        public const string DeleteComment = $"{Base}/{{recordingId}}/comments/{{commentId}}";

        public const string GetTags = $"{Base}/{{recordingId}}/tags";
        public const string PostDeleteTag = $"{Base}/{{recordingId}}/tags/{{tag}}";
    }

    public static class Tags
    {
        private const string Base = $"{ApiVersion}/tags";
        public const string GetMostUsed = $"{Base}/mostused/{{limit}}";
    }
    public static class Audits
    {
        private const string Base = "AuditReport";
        public const string LoginUrl = "https://greshamhouseie.clarifygo.com/Account/Login"; // Add this line
        public static string Search(string fromDate, string toDate, string eventType = "All+Events", int offset = 0, int maxResults = 10000)
        {
            return $"{Base}/Search?fromDate={fromDate}&toDate={toDate}&eventType={eventType}&offset={offset}&maxResults={maxResults}";
        }
    }
}