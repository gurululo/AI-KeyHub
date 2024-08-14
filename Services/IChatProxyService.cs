using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public interface IChatProxyService
{
    Task ForwardChatCompletionAsync(
        HttpContext context, 
        Channel channel, 
        string rawRequestBody, 
        bool isStream, 
        string rawClientApiKey, 
        CancellationToken cancellationToken);

    string NormalizeTargetUrl(string baseUrl);
    string SanitizeOutput(string text, params string?[] sensitiveValues);
}
