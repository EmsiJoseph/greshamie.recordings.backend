using System.Collections.Specialized;
using System.Web;
using backend.Exceptions;

namespace backend.Services;

public class ClarifyGoService
{
    private readonly HttpClient _httpClient;

    public ClarifyGoService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient("ClarifyGo");
    }

    public async Task<HttpResponseMessage> ForwardRequest(
        HttpMethod method,
        string endpoint,
        HttpContent content = null,
        NameValueCollection query = null)
    {
        var request = new HttpRequestMessage(method, endpoint)
        {
            Content = content
        };

        if (query != null)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(query);
            request.RequestUri = new UriBuilder(request.RequestUri)
            {
                Query = queryString.ToString()
            }.Uri;
        }

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ClarifyGoException(
                response.StatusCode,
                $"ClarifyGo API returned an error: {response.ReasonPhrase}",
                errorContent
            );
        }

        return response;
    }
}