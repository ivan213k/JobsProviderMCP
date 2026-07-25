using System.Text.Json.Serialization;

namespace ApifySdk.Actors.Indeed.Models;

public class IndeedSearchRequest
{
    [JsonPropertyName("title")]
    public string Keywords { get; set; } = null!;

    /// <summary>ISO 3166-1 alpha-2 country code selecting the regional job board, e.g. <c>"de"</c>.</summary>
    public string Country { get; set; } = null!;

    /// <summary>Single location to search in, e.g. <c>"Leipzig"</c>. " " searches the whole country.</summary>
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("datePosted")]
    public string MaxAgeOfPostingInDays { get; set; } = null!;

    /// <summary>Maximum number of jobs the actor should return.</summary>
    public int Limit { get; set; }
}
