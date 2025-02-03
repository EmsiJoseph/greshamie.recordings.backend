namespace backend.Constants;

public abstract class BaseApiEndpoints
{
    protected readonly string Base;

    protected BaseApiEndpoints(string basePath)
    {
        Base = basePath;
    }

    public static class Comments
    {
        protected static object GetEndpoints(string basePath, string recordingId) => 
        {
            var commentBase = $"{basePath}/{recordingId}/comments";
            return new
            {
                GetAll = commentBase,
                Add = commentBase,
                Delete = $"{commentBase}/{{commentId}}"
            };
        }
    }

    public static class Tags
    {
        protected static object GetEndpoints(string basePath, string recordingId) =>
        {
            var tagBase = $"{basePath}/{recordingId}/tags";
            return new
            {
                GetAll = tagBase,
                Add = $"{tagBase}/{{tag}}",
                Remove = $"{tagBase}/{{tag}}"
            };
        }
    }
}
