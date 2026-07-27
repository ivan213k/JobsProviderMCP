namespace JobsProviderApi.Models;

/// <summary>
/// <paramref name="TotalCount"/> is the count of all jobs matching every filter, before
/// <c>skip</c>/<c>take</c> pagination is applied — it is not <c>Items.Count</c>.
/// <paramref name="Items"/> is just the requested page.
/// </summary>
public record ListResponse<T>(int TotalCount, IReadOnlyList<T> Items);
