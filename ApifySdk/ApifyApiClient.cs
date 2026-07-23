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
    
    public async Task<IEnumerable<T>> RunActorAsync<T>(string actorId, object input, CancellationToken cancellationToken = default)
    {
        // run-sync-get-dataset-items starts the run, waits for it, and returns the dataset items.
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/acts/{actorId}/run-sync-get-dataset-items")
            {
                Content = JsonContent.Create(input)
            };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Apify actor '{actorId}' returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}",
                inner: null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<IEnumerable<T>>(cancellationToken) ?? [];
    }
}
