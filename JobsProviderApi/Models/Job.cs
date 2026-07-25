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
    DateTime DatePublished);
