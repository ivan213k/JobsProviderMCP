namespace ApifySdk.Actors.LinkedIn.Models;

public class LinkedInJobResult
{
    public string JobId { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public string JobUrl { get; set; } = null!;

    public DateTime PublishedAt { get; set; }

    public string? Location { get; set; }

    public string? CompanyName { get; set; }

    public string JobDescription { get; set; } = null!;

    /// <summary>
    /// The subset of the request's <c>resumeKeywords</c> found in this job's description. Only populated when the
    /// request carried keywords — the actor omits it entirely otherwise.
    /// </summary>
    public string[]? MatchedKeywords { get; set; }

    /// <summary>The request's <c>resumeKeywords</c> that were NOT found in this job's description.</summary>
    public string[]? UnmatchedKeywords { get; set; }

    /// <summary>Share of the request's <c>resumeKeywords</c> that matched, 0-100.</summary>
    public int KeywordMatchScorePercentage { get; set; }
}
