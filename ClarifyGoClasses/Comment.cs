namespace backend.ClarifyGoClasses;

public class Comment
{
    public string? Id { get; set; }
    public string? RecordingId { get; set; }
    public DateTime? Created { get; set; }
    public string? Value { get; set; }
    public string? Username { get; set; }
    public string? LabelHeaderString { get; set; }
}