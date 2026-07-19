namespace JobsProviderApi.Models;

public record Job(
    int Id,
    string Title,
    string Company,
    string Location,
    string Description,
    IReadOnlyList<string> Requirements,
    string Link,
    string SourcingPlatform,
    string DatePublished);
