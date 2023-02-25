using System.Text.Json.Serialization;

namespace NGitLab.Models;

public class MergeRequestVersion
{
    [JsonPropertyName("head_commit_sha")]
    public string HeadCommitSha { get; set; }

    [JsonPropertyName("base_commit_sha")]
    public string BaseCommitSha { get; set; }

    [JsonPropertyName("start_commit_sha")]
    public string StartCommitSha { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("merge_request_id")]
    public int MergeRequestId { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("real_size")]
    public long RealSize { get; set; }
}