using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using affolterNET.Auth.Bff.Configuration;

namespace affolterNET.Auth.Bff.Services;

public class BffApiClient : IBffApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BffApiClient> _logger;
    private readonly BffAuthOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public BffApiClient(
        HttpClient httpClient,
        ILogger<BffApiClient> logger,
        IOptions<BffAuthOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send HTTP request to {Method} {Uri}", request.Method, request.RequestUri);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint, string? accessToken = null, CancellationToken cancellationToken = default) where T : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await SendAsync(request, accessToken, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GET request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {Endpoint}", endpoint);
            return null;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string? accessToken = null, CancellationToken cancellationToken = default) 
        where TRequest : class 
        where TResponse : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using var response = await SendAsync(request, accessToken, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("POST request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {Endpoint}", endpoint);
            return null;
        }
    }
}