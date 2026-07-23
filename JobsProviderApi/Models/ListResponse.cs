namespace JobsProviderApi.Models;

public record ListResponse<T>(int TotalCount, IReadOnlyList<T> Items);
