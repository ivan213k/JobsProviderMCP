namespace ApifySdk.Actors.Indeed.Models;

public class IndeedSearchRequest
{
    public string Title { get; set; } = null!;

    /// <summary>Single location to search in, e.g. <c>"Leipzig"</c>. " " searches the whole country.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 country code selecting the regional job board, e.g. <c>"de"</c>.</summary>
    public string Country { get; set; } = null!;

    /// <summary>Maximum age of a posting in days, sent as a string, e.g. <c>"7"</c>.</summary>
    public string DatePosted { get; set; } = null!;

    /// <summary>Maximum number of jobs the actor should return.</summary>
    public int Limit { get; set; }
}
