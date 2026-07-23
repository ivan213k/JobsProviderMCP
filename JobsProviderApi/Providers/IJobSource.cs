namespace JobsProviderApi.Providers;

/// <summary>
/// Marker for a job source. Implementations are never instantiated — they only give
/// <see cref="IJobsProvider{TSource}"/> a distinct type per source.
/// </summary>
public interface IJobSource;

public sealed class IndeedSource : IJobSource;

public sealed class StepstoneSource : IJobSource;
