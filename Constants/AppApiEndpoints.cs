namespace backend.Constants;

public static class AppApiEndpoints
{
    public static class HistoricRecordings
    {
        private const string BasePath = "/api/recordings/historic";
        public const string Search = $"{BasePath}/search/{{startDate}}/{{endDate}}";
        public const string Delete = $"{BasePath}/{{recordingId}}";
        public const string ExportMp3 = $"{BasePath}/{{recordingId}}/mp3";
        public const string ExportWav = $"{BasePath}/{{recordingId}}/wav";

        public static class Comments
        {
            private static readonly (string GetAll, string Add, string Delete) Paths = 
                EndpointUtilities.CommentEndpoints.GetPaths(BasePath);
            
            public static string GetAll => Paths.GetAll;
            public static string Add => Paths.Add;
            public static string Delete => Paths.Delete;
        }

        public static class Tags
        {
            private static readonly (string GetAll, string Add, string Remove) Paths = 
                EndpointUtilities.TagEndpoints.GetPaths(BasePath);
            
            public static string GetAll => Paths.GetAll;
            public static string Add => Paths.Add;
            public static string Remove => Paths.Remove;
        }
    }

    public static class LiveRecordings
    {
        private const string Base = "/api/recordings/live";
        public const string GetAll = Base;
        public const string Resume = $"{Base}/{{recorderId}}/{{recordingId}}/resume";
        public const string Pause = $"{Base}/{{recorderId}}/{{recordingId}}/pause";
        
        public static class Comments
        {
            public const string GetAll = $"{Base}/{{recordingId}}/comments";
            public const string Add = $"{Base}/{{recordingId}}/comments";
            public const string Delete = $"{Base}/{{recordingId}}/comments/{{commentId}}";
        }
        
        public static class Tags
        {
            public const string Add = $"{Base}/{{recordingId}}/tags/{{tag}}";
            public const string Remove = $"{Base}/{{recordingId}}/tags/{{tag}}";
        }
    }

    public static class Tags
    {
        private const string Base = "/api/tags";
        public const string MostUsed = $"{Base}/mostused/{{limit}}";
    }
}