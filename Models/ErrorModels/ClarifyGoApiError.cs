using System.Net;
using System.Text.Json.Serialization;

namespace backend.Models.ErrorModels;

public class ClarifyGoApiError
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("error_description")]
    public string Description { get; set; }

    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }
}