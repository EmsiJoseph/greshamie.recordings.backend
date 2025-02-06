using System.Text.Json.Serialization;

namespace backend.Classes;

public class ClarifyGoHistoricRecordingRaw
{
    // May be null.
    [JsonPropertyName("accountId")] public string? AccountId { get; set; }

    [JsonPropertyName("pbxId")] public string? PbxId { get; set; }

    // List of PBX account endpoints.
    [JsonPropertyName("pbxAccountEndpoints")]
    public List<PbxAccountEndpoint>? PbxAccountEndpoints { get; set; }

    [JsonPropertyName("mediaCompletedTime")]
    public DateTime? MediaCompletedTime { get; set; }

    [JsonPropertyName("alertedTime")] public DateTime? AlertedTime { get; set; }

    [JsonPropertyName("connectedTime")] public DateTime? ConnectedTime { get; set; }

    [JsonPropertyName("disconnectedTime")] public DateTime? DisconnectedTime { get; set; }

    [JsonPropertyName("mediaServerId")] public string? MediaServerId { get; set; }

    [JsonPropertyName("recorderClusterId")]
    public string? RecorderClusterId { get; set; }

    [JsonPropertyName("recordingGroupingId")]
    public string? RecordingGroupingId { get; set; }

    [JsonPropertyName("directRecordingLink")]
    public string? DirectRecordingLink { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("recorderId")] public string? RecorderId { get; set; }

    // This property is left as object, since its structure is not defined.
    [JsonPropertyName("pbxAccounts")] public object? PbxAccounts { get; set; }

    [JsonPropertyName("callType")] public int CallType { get; set; }

    [JsonPropertyName("calledParty")] public string? CalledParty { get; set; }

    [JsonPropertyName("callingParty")] public string? CallingParty { get; set; }

    [JsonPropertyName("state")] public int State { get; set; }

    [JsonPropertyName("channel")] public int Channel { get; set; }

    [JsonPropertyName("mediaStartedTime")] public DateTime? MediaStartedTime { get; set; }

    [JsonPropertyName("isHidden")] public bool IsHidden { get; set; }
}

// Represents each PBX account endpoint.
public class PbxAccountEndpoint
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("recordingId")] public string? RecordingId { get; set; }

    [JsonPropertyName("pbxAccountId")] public string? PbxAccountId { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("number")] public string? Number { get; set; }

    [JsonPropertyName("huntGroupName")] public string? HuntGroupName { get; set; }

    [JsonPropertyName("huntGroupNumber")] public string? HuntGroupNumber { get; set; }

    [JsonPropertyName("alertedTime")] public DateTime? AlertedTime { get; set; }

    [JsonPropertyName("connectedTime")] public DateTime? ConnectedTime { get; set; }

    [JsonPropertyName("disconnectedTime")] public DateTime? DisconnectedTime { get; set; }
}