namespace backend.Classes;

public class ApiSettings
{
    public const string SectionName = "ClarifyGoAPI";
    public string IdentityServerUri { get; set; }
    public string ApiBaseUri { get; set; }
}