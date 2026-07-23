namespace ApifySdk;

public class ApifyOptions
{
    public string BaseUrl { get; init; } = null!;

    public string Token { get; init; } = null!;

    public TimeSpan RequestTimeout { get; init; }
}
