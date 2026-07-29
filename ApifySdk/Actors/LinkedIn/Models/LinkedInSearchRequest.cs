using System.Text.Json.Serialization;

namespace ApifySdk.Actors.LinkedIn.Models;

public class LinkedInSearchRequest
{
    [JsonPropertyName("keyword")]
    public string[] Keywords { get; set; } = null!;

    /// <summary>Locations to search in, e.g. <c>["Berlin"]</c>. Each location is combined with every keyword.</summary>
    public string[] Locations { get; set; } = [];

    /// <summary>
    /// Skills to score each job against. The response echoes the ones found in a job's description back as
    /// <c>matchedKeywords</c>, which is what this source uses in place of a requirements field.
    /// </summary>
    public ResumeKeyword[] ResumeKeywords { get; set; } = [];

    /// <summary>Maximum age of a posting. There is no free-form max-age input.</summary>
    public Published PublishedAt { get; set; }

    /// <summary>Maximum number of jobs the actor should return.</summary>
    public int MaxItems { get; set; }

    /// <summary>Drops duplicate postings, by job id, before they reach the dataset.</summary>
    public bool SaveOnlyUniqueItems { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<Published>))]
public enum Published
{
    [JsonStringEnumMemberName("r86400")]
    Past24Hours,

    [JsonStringEnumMemberName("r604800")]
    Past7Days,

    [JsonStringEnumMemberName("r2592000")]
    Past30Days
}

public class ResumeKeyword
{
    public string Keyword { get; set; } = null!;

    /// <summary>
    /// Alternative spellings that also count as a match, e.g. <c>["K8s"]</c> for <c>"Kubernetes"</c>. The actor
    /// rejects the run outright if this is null, so it stays an empty array while aliases are unused.
    /// </summary>
    public string[] Aliases { get; set; } = [];
}
