using System;
using System.Net;

namespace backend.Constants
{
    public static class ClarifyGoApiEndpoints
    {
        // Base URL for the API (from the OpenAPI spec, it's "/API")
        private const string ApiBase = "/API";

        // API version used in all endpoints
        private const string Version = "v1.0";

        public static class HistoricRecordings
        {
            // Base path for historic recordings endpoints
            private const string Base = $"{ApiBase}/{Version}/historicrecordings";

            // Builds the URL to search historic recordings using start and end dates.
            public static string Search(DateTime startDate, DateTime endDate)
            {
                // Format dates as ISO 8601 UTC strings.
                string start = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                string end = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                return $"{Base}/{start}/{end}";
            }

            // URL to delete a historic recording.
            public static string Delete(string recordingId) =>
                $"{Base}/{recordingId}";

            // URL to export a recording's audio as MP3.
            public static string ExportMp3(string recordingId) =>
                $"{Base}/{recordingId}/Mp3";

            // URL to export a recording's audio as WAV.
            public static string ExportWav(string recordingId) =>
                $"{Base}/{recordingId}/Wav";

            // URL to retrieve comments for a historic recording.
            public static string GetComments(string recordingId) =>
                $"{Base}/{recordingId}/comments";

            // URL to add a comment to a historic recording (comment text passed as a query parameter).
            public static string AddComment(string recordingId, string notes) =>
                $"{Base}/{recordingId}/comments?notes={WebUtility.UrlEncode(notes)}";

            // URL to delete a specific comment from a historic recording.
            public static string DeleteComment(string recordingId, string commentId) =>
                $"{Base}/{recordingId}/comments/{commentId}";

            // URL to get all tags associated with a historic recording.
            public static string GetTags(string recordingId) =>
                $"{Base}/{recordingId}/tags";

            // URL to add a tag to a historic recording.
            public static string AddTag(string recordingId, string tag) =>
                $"{Base}/{recordingId}/tags/{WebUtility.UrlEncode(tag)}";

            // URL to remove a tag from a historic recording.
            public static string DeleteTag(string recordingId, string tag) =>
                $"{Base}/{recordingId}/tags/{WebUtility.UrlEncode(tag)}";
        }

        public static class LiveRecordings
        {
            // Base path for live recordings endpoints.
            private const string Base = $"{ApiBase}/{Version}/liverecordings";

            // URL to get all live recordings.
            public static string GetAll() => Base;

            // URL to retrieve comments for a live recording.
            public static string GetComments(string recordingId) =>
                $"{Base}/{recordingId}/comments";

            // URL to add a comment to a live recording.
            public static string AddComment(string recordingId, string notes) =>
                $"{Base}/{recordingId}/comments?notes={WebUtility.UrlEncode(notes)}";

            // URL to delete a comment from a live recording.
            public static string DeleteComment(string recordingId, string commentId) =>
                $"{Base}/{recordingId}/comments/{commentId}";

            // URL to get tags for a live recording.
            public static string GetTags(string recordingId) =>
                $"{Base}/{recordingId}/tags";

            // URL to add a tag to a live recording.
            public static string AddTag(string recordingId, string tag) =>
                $"{Base}/{recordingId}/tags/{WebUtility.UrlEncode(tag)}";

            // URL to remove a tag from a live recording.
            public static string DeleteTag(string recordingId, string tag) =>
                $"{Base}/{recordingId}/tags/{WebUtility.UrlEncode(tag)}";

            // URL to resume a paused live recording.
            public static string Resume(string recorderId, string recordingId) =>
                $"{Base}/{recorderId}/{recordingId}/resume";

            // URL to pause an active live recording.
            public static string Pause(string recorderId, string recordingId) =>
                $"{Base}/{recorderId}/{recordingId}/pause";
        }

        public static class Tags
        {
            // Base path for tag endpoints.
            private const string Base = $"{ApiBase}/{Version}/tags";

            // URL to retrieve the most commonly used tags with a given limit.
            public static string MostUsed(int limit) =>
                $"{Base}/mostused/{limit}";
        }
    }
}