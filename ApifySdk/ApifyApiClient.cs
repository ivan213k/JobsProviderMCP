using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApifySdk;

public class ApifyApiClient : IApifyApiClient
{
    private readonly HttpClient _httpClient;

    private readonly ApifyOptions _options;

    public ApifyApiClient(HttpClient httpClient, ApifyOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }
    
    public async Task<IEnumerable<TResponse>> PostAsync<TResponse, TSearchQuery>(string actorId, TSearchQuery query, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = BuildRequest(actorId, query);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessStatusCode(response, actorId, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IEnumerable<TResponse>>(cancellationToken) ?? [];
    }

    private HttpRequestMessage BuildRequest<T>(string actorId, T? query)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/acts/{actorId}/run-sync-get-dataset-items")
        {
            Content = JsonContent.Create(query)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
        return request;
    }

    private static async Task EnsureSuccessStatusCode(HttpResponseMessage response, string actorId, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Apify actor '{actorId}' returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}",
            inner: null,
            response.StatusCode);
    }
}
