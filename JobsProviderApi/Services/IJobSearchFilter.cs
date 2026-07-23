using JobsProviderApi.Models;

namespace JobsProviderApi.Services;

public interface IJobSearchFilter
{
    ListResponse<Job> Apply(IEnumerable<Job> jobs, JobSearchQuery query);
}
