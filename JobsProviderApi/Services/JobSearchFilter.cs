using JobsProviderApi.Models;

namespace JobsProviderApi.Services;

/// <summary>
/// Filtering shared by the per-source job services: must-have/preferred skill and preferred-location matching,
/// sorted newest-first by <see cref="Job.DatePublished"/>, with skip/take pagination applied last.
/// </summary>
public class JobSearchFilter : IJobSearchFilter
{
    public ListResponse<Job> Apply(IEnumerable<Job> jobs, JobSearchQuery query)
    {
        string[] mustHaveSkills = NormalizeForComparing(query.MustHaveSkills);
        string[] preferredSkills = NormalizeForComparing(query.PreferredSkills);
        string[] locations = NormalizeForComparing(query.Locations);

        if (mustHaveSkills.Any())
        {
            jobs = jobs.Where(job => mustHaveSkills.All(skill => HasSkill(job, skill)));
        }

        if (preferredSkills.Any())
        {
            jobs = jobs.Where(job => preferredSkills.Any(skill => HasSkill(job, skill)));
        }

        if (locations.Any())
        {
            jobs = jobs.Where(job => locations.Any(location => IsInLocation(job, location)));
        }

        List<Job> matched = jobs.OrderByDescending(ParseDatePublished).ToList();
        List<Job> page = matched.Skip(query.Skip).Take(query.Take).ToList();
        return new ListResponse<Job>(matched.Count, page);
    }

    private static DateOnly ParseDatePublished(Job job) =>
        DateOnly.TryParse(job.DatePublished, out DateOnly datePublished) ? datePublished : DateOnly.MinValue;

    private static bool HasSkill(Job job, string normalizedSkill)
    {
        if (job.Requirements is null)
        {
            return false;
        }

        return job.Requirements.Any(requirement =>
            string.Equals(requirement.Trim(), normalizedSkill, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsInLocation(Job job, string normalizedLocation) =>
        string.IsNullOrWhiteSpace(job.Location) ||
        job.Location.Contains(normalizedLocation, StringComparison.OrdinalIgnoreCase);

    private static string[] NormalizeForComparing(string[]? values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray() ?? [];
}
