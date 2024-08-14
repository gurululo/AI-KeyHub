using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI_KeyHub.Models;
using AI_KeyHub.Models.Api;

namespace AI_KeyHub.Services;

public class ChatProxyService : IChatProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatProxyService> _logger;

    public ChatProxyService(IHttpClientFactory httpClientFactory, ILogger<ChatProxyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string NormalizeTargetUrl(string baseUrl)
    {
        var trimmed = baseUrl.Trim().TrimEnd('/');
        if (trimmed.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }
        if (trimmed.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            return $"{trimmed}/chat/completions";
        }
        return $"{trimmed}/v1/chat/completions";
    }

    public string SanitizeOutput(string text, params string?[] sensitiveValues)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = text;
        foreach (var val in sensitiveValues)
        {
            if (!string.IsNullOrWhiteSpace(val) && val.Length >= 4)
            {
                result = result.Replace(val, "[REDACTED]");
            }
        }
        return result;
    }

    public async Task ForwardChatCompletionAsync(
        HttpContext context,
        Channel channel,
        string rawRequestBody,
        bool isStream,
        string rawClientApiKey,
        CancellationToken cancellationToken)
    {
        var targetUrl = NormalizeTargetUrl(channel.BaseUrl);
        var httpClient = _httpClientFactory.CreateClient("UpstreamClient");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        requestMessage.Content = new StringContent(rawRequestBody, Encoding.UTF8, "application/json");

        // 配置上游认证凭据，严禁透传客户端密钥
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", channel.ApiKey);

        HttpResponseMessage upstreamResponse;
        try
        {
            upstreamResponse = await httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Client cancelled the request to {TargetUrl}", targetUrl);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send request to upstream {TargetUrl}", targetUrl);
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            context.Response.ContentType = "application/json; charset=utf-8";
            
            var err = OpenAiErrorResponse.Create(
                $"Failed to connect to upstream service: {SanitizeOutput(ex.Message, channel.ApiKey, rawClientApiKey)}",
                type: "api_error",
                code: "upstream_connect_error");
            await context.Response.WriteAsync(JsonSerializer.Serialize(err), cancellationToken);
            return;
        }

        context.Response.StatusCode = (int)upstreamResponse.StatusCode;

        if (isStream && upstreamResponse.IsSuccessStatusCode)
        {
            context.Response.ContentType = "text/event-stream; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Append("X-Accel-Buffering", "no");

            await using var upstreamStream = await upstreamResponse.Content.ReadAsStreamAsync(cancellationToken);
            var buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = await upstreamStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await context.Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                await context.Response.Body.FlushAsync(cancellationToken);
            }
        }
        else
        {
            var rawContent = await upstreamResponse.Content.ReadAsStringAsync(cancellationToken);
            var sanitizedContent = SanitizeOutput(rawContent, channel.ApiKey, rawClientApiKey);

            context.Response.ContentType = upstreamResponse.Content.Headers.ContentType?.ToString() ?? "application/json; charset=utf-8";
            await context.Response.WriteAsync(sanitizedContent, Encoding.UTF8, cancellationToken);
        }
    }
}
