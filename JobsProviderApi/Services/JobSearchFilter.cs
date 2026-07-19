using System.Text.RegularExpressions;
using JobsProviderApi.Models;

namespace JobsProviderApi.Services;

/// <summary>
/// Filtering shared by the per-source job services: free-text/regex search plus must-have/preferred skill matching.
/// </summary>
public class JobSearchFilter : IJobSearchFilter
{
    public IReadOnlyList<Job> Apply(IEnumerable<Job> jobs, JobSearchQuery query)
    {
        var regex = new Regex(query.Search, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        IEnumerable<Job> result = jobs.Where(job => regex.IsMatch(job.Title) || regex.IsMatch(job.Description));

        if (query.MustHaveSkills is { Length: > 0 })
        {
            result = result.Where(job => query.MustHaveSkills.All(skill => HasSkill(job, skill)));
        }

        if (query.PreferredSkills is { Length: > 0 })
        {
            result = result.Where(job => query.PreferredSkills.Any(skill => HasSkill(job, skill)));
        }

        return result.Take(query.Take).ToList();
    }

    private static bool HasSkill(Job job, string skill)
    {
        if(job.Requirements is null)
        {
            return false;
        }

        return job.Requirements.Any(requirement => string.Equals(requirement.Trim(), skill.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
