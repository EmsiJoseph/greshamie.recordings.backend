namespace backend.Constants;

public static class EndpointUtilities
{
    public static class CommentEndpoints
    {
        public static (string GetAll, string Add, string Delete) GetPaths(string basePath, string paramName = "recordingId") =>
        (
            GetAll: $"{basePath}/{{{paramName}}}/comments",
            Add: $"{basePath}/{{{paramName}}}/comments",
            Delete: $"{basePath}/{{{paramName}}}/comments/{{commentId}}"
        );
    }

    public static class TagEndpoints
    {
        public static (string GetAll, string Add, string Remove) GetPaths(string basePath, string paramName = "recordingId") =>
        (
            GetAll: $"{basePath}/{{{paramName}}}/tags",
            Add: $"{basePath}/{{{paramName}}}/tags/{{tag}}",
            Remove: $"{basePath}/{{{paramName}}}/tags/{{tag}}"
        );
    }
}
