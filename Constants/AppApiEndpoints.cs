using System.Net;

namespace backend.Constants;

public static class AppApiEndpoints
{
    private const string ApiBase = "/api";

    public static class Recordings
    {
        private const string Base = $"{ApiBase}/recordings";

        public static class Historic
        {
            private const string HistoricBase = $"{Base}";
            
            public static string GetAll = HistoricBase;
            
            public static string Search = $"{HistoricBase}/search";
            
            public static string Delete = $"{HistoricBase}/{{recordingId}}";


            public static string ExportMp3 = $"{HistoricBase}/{{recordingId}}/mp3";

            public static string ExportWav = $"{HistoricBase}/{{recordingId}}/wav";


            public static string Comments = $"{HistoricBase}/{{recordingId}}/comments";

            public static string CommentById = $"{HistoricBase}/{{recordingId}}/comments/{{commentId}}";

            public static string Tags = $"{HistoricBase}/{{recordingId}}/tags";

            public static string TagOperations = $"{HistoricBase}/{{recordingId}}/tags/{{tag}}";
        }

        public static class Live
        {
            private const string LiveBase = $"{Base}/live";

            public const string GetAll = LiveBase;

            public const string Resume = $"{LiveBase}/{{recorderId}}/{{recordingId}}/resume";

            public static string Pause = $"{LiveBase}/{{recorderId}}/{{recordingId}}/pause";

            public static string Comments = $"{LiveBase}/{{recordingId}}/comments";

            public static string CommentById = $"{LiveBase}/{{recordingId}}/comments/{{commentId}}";

            public static string Tags = $"{LiveBase}/{{recordingId}}/tags";

            public static string TagOperations = $"{LiveBase}/{{recordingId}}/tags/{{tag}}";
        }
    }

    public static class Tags
    {
        private const string Base = $"{ApiBase}/tags";

        public static string MostUsed = $"{Base}/mostused/{{limit}}";
    }
}