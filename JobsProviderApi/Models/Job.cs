namespace JobsProviderApi.Models;

public record Job(
    string Id,
    string Title,
    string? Company,
    string? Location,
    string Description,
    IReadOnlyList<string>? Requirements,
    string Link,
    string SourcingPlatform,
    string DatePublished)
{
    public static string ToCacheKey(string source, string id) => $"{source}:job:{id}";
}
