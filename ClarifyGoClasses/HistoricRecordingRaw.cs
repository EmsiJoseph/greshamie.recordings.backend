using System.Text.Json.Serialization;

namespace backend.ClarifyGoClasses;

public class HistoricRecordingRaw
{
    // May be null.
    public string? AccountId { get; set; }

    public string? PbxId { get; set; }

    // List of PBX account endpoints.

    public List<PbxAccountEndpoint>? PbxAccountEndpoints { get; set; }
    public DateTime MediaCompletedTime { get; set; }

    public DateTime? AlertedTime { get; set; }

    public DateTime? ConnectedTime { get; set; }

    public DateTime? DisconnectedTime { get; set; }

    public string? MediaServerId { get; set; }


    public string? RecorderClusterId { get; set; }


    public string? RecordingGroupingId { get; set; }


    public string? DirectRecordingLink { get; set; }

    public string? Id { get; set; }

    public string? RecorderId { get; set; }

    // This property is left as object, since its structure is not defined.
    public object? PbxAccounts { get; set; }

    public int? CallType { get; set; }

    public string? CalledParty { get; set; }

    public string? CallingParty { get; set; }

    public int State { get; set; }

    public int Channel { get; set; }

    public DateTime MediaStartedTime { get; set; }

    public bool IsHidden { get; set; }
}

// Represents each PBX account endpoint.
public  class PbxAccountEndpoint
{
    public string? Id { get; set; }

    public string? RecordingId { get; set; }

    public string? PbxAccountId { get; set; }

    public string? Name { get; set; }

    public string? Number { get; set; }

    public string? HuntGroupName { get; set; }

    public string? HuntGroupNumber { get; set; }

    public DateTime? AlertedTime { get; set; }

    public DateTime? ConnectedTime { get; set; }

    public DateTime? DisconnectedTime { get; set; }
}