using ModelContextProtocol.Protocol;

namespace JobsProviderApi.Configuration;

public static class McpSetup
{
    public static IServiceCollection AddJobsMcp(this IServiceCollection services, string version)
    {
        services.AddMcpServer(options =>
        {
            options.ServerInfo = new Implementation { Name = "JobsProviderApi", Version = version };
        }).WithHttpTransport(options => options.Stateless = true).WithToolsFromAssembly();

        return services;
    }
}
