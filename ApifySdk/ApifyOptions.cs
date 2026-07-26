namespace ApifySdk;

public class ApifyOptions
{
    public const string SectionName = "Apify";
    
    public string BaseUrl { get; init; } = null!;

    public string Token { get; init; } = null!;

    public int TimeoutInSeconds { get; init; }
}
