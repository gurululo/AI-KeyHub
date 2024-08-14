using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AI_KeyHub.Models.Api;
using AI_KeyHub.Services;

namespace AI_KeyHub.Controllers;

[ApiController]
[AllowAnonymous]
public class ApiController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IChannelService _channelService;
    private readonly IChatProxyService _chatProxyService;
    private readonly ILogger<ApiController> _logger;

    public ApiController(
        IApiKeyService apiKeyService,
        IChannelService channelService,
        IChatProxyService chatProxyService,
        ILogger<ApiController> logger)
    {
        _apiKeyService = apiKeyService;
        _channelService = channelService;
        _chatProxyService = chatProxyService;
        _logger = logger;
    }

    [HttpPost("/v1/chat/completions")]
    [HttpPost("v1/chat/completions")]
    public async Task<IActionResult> ChatCompletions()
    {
        // 1. 验证客户端 Authorization Header 凭证
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
        {
            return Unauthorized(OpenAiErrorResponse.Create(
                "Missing Authorization header in request. Please provide Bearer token.",
                code: "missing_api_key"));
        }

        var authStr = authHeader.ToString().Trim();
        if (!authStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(OpenAiErrorResponse.Create(
                "Invalid Authorization header format. Format should be 'Bearer <your_api_key>'.",
                code: "invalid_auth_header"));
        }

        var rawKey = authStr["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(rawKey))
        {
            return Unauthorized(OpenAiErrorResponse.Create(
                "API key cannot be empty.",
                code: "empty_api_key"));
        }

        // 2. 校验内部 API Key 真实性与启用状态
        var internalKey = _apiKeyService.ValidateAndGetApiKey(rawKey);
        if (internalKey == null || !internalKey.IsEnabled)
        {
            return Unauthorized(OpenAiErrorResponse.Create(
                "Incorrect API key provided or the API key has been disabled/deleted.",
                code: "invalid_api_key"));
        }

        // 3. 校验绑定的渠道状态
        var channel = _channelService.GetChannelById(internalKey.ChannelId);
        if (channel == null || !channel.IsEnabled)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, OpenAiErrorResponse.Create(
                "The upstream AI channel bound to this API key is currently unavailable or disabled.",
                type: "channel_unavailable_error",
                code: "channel_disabled"));
        }

        // 4. 解析并校验请求体
        string rawRequestBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            rawRequestBody = await reader.ReadToEndAsync(HttpContext.RequestAborted);
        }

        if (string.IsNullOrWhiteSpace(rawRequestBody))
        {
            return BadRequest(OpenAiErrorResponse.Create(
                "Request body cannot be empty.",
                code: "empty_request_body"));
        }

        JsonDocument jsonDoc;
        try
        {
            jsonDoc = JsonDocument.Parse(rawRequestBody);
        }
        catch (JsonException)
        {
            return BadRequest(OpenAiErrorResponse.Create(
                "Invalid JSON payload in request body.",
                code: "invalid_json"));
        }

        using (jsonDoc)
        {
            var root = jsonDoc.RootElement;
            if (!root.TryGetProperty("model", out var modelProperty) || string.IsNullOrWhiteSpace(modelProperty.GetString()))
            {
                return BadRequest(OpenAiErrorResponse.Create(
                    "Missing required 'model' parameter in request body.",
                    code: "missing_model_parameter",
                    param: "model"));
            }

            var requestedModel = modelProperty.GetString()!.Trim();

            // 5. 校验请求模型与 Key 绑定渠道模型是否匹配（需求 4.5）
            if (!string.Equals(requestedModel, channel.Model, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(OpenAiErrorResponse.Create(
                    $"Model '{requestedModel}' is not supported by this API Key's bound channel. Configured model for this key is '{channel.Model}'.",
                    code: "model_mismatch",
                    param: "model"));
            }

            bool isStream = false;
            if (root.TryGetProperty("stream", out var streamProp) && streamProp.ValueKind == JsonValueKind.True)
            {
                isStream = true;
            }

            // 6. 转发至上游服务商
            await _chatProxyService.ForwardChatCompletionAsync(
                HttpContext,
                channel,
                rawRequestBody,
                isStream,
                rawKey,
                HttpContext.RequestAborted);

            return new EmptyResult();
        }
    }
}
