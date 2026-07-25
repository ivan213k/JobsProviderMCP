namespace JobsProviderApi.Configuration;

public class CachingOptions
{
    public const string SectionName = "Caching";

    public TimeSpan SearchResultsDuration { get; set; } = TimeSpan.FromHours(3);
    public TimeSpan JobDuration { get; set; } = TimeSpan.FromDays(7);
}
