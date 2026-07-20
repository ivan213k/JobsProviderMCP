using System.Text.RegularExpressions;
using JobsProviderApi.Models;

namespace JobsProviderApi.Services;

/// <summary>
/// Filtering shared by the per-source job services: free-text/regex search plus must-have/preferred skill and
/// preferred-location matching.
/// </summary>
public class JobSearchFilter : IJobSearchFilter
{
    public IReadOnlyList<Job> Apply(IEnumerable<Job> jobs, JobSearchQuery query)
    {
        var regex = new Regex(query.Search, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        IEnumerable<Job> result = jobs.Where(job => regex.IsMatch(job.Title) || regex.IsMatch(job.Description));

        string[] mustHaveSkills = NormalizeForComparing(query.MustHaveSkills);
        string[] preferredSkills = NormalizeForComparing(query.PreferredSkills);
        string[] locations = NormalizeForComparing(query.Locations);

        if (mustHaveSkills.Any())
        {
            result = result.Where(job => mustHaveSkills.All(skill => HasSkill(job, skill)));
        }

        if (preferredSkills.Any())
        {
            result = result.Where(job => preferredSkills.Any(skill => HasSkill(job, skill)));
        }

        if (locations.Any())
        {
            result = result.Where(job => locations.Any(location => IsInLocation(job, location)));
        }

        return result.Take(query.Take).ToList();
    }

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
        job.Location.Contains(normalizedLocation, StringComparison.OrdinalIgnoreCase);

    private static string[] NormalizeForComparing(string[]? values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray() ?? [];
}
