using JobsProviderApi.Configuration;
using Microsoft.Extensions.Options;

namespace JobsProviderApi.Tests.Fakes;

internal static class TestCachingOptions
{
    public static IOptions<CachingOptions> Default() => Options.Create(new CachingOptions());
}
