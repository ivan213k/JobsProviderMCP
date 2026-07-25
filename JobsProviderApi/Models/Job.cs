namespace JobsProviderApi.Models;

public record Job(
    int Id,
    string Title,
    string Company,
    string Location,
    string Description,
    IReadOnlyList<string>? Requirements,
    string Link,
    string SourcingPlatform,
    string DatePublished)
{
    public static string ToCacheKey(string source, int id) => $"{source}:job:{id}";
}
