using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace JobsProviderApi.Tests.Fakes;

internal static class TestFusionCache
{
    public static IFusionCache Create() =>
        new ServiceCollection().AddFusionCache().Services.BuildServiceProvider().GetRequiredService<IFusionCache>();
}
