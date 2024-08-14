using System.Text.Json.Serialization;

namespace AI_KeyHub.Models.Api;

public class OpenAiErrorResponse
{
    [JsonPropertyName("error")]
    public OpenAiError Detail { get; set; } = new();

    public static OpenAiErrorResponse Create(string message, string type = "invalid_request_error", string? code = null, string? param = null)
    {
        return new OpenAiErrorResponse
        {
            Detail = new OpenAiError
            {
                Message = message,
                Type = type,
                Code = code,
                Param = param
            }
        };
    }
}

public class OpenAiError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "invalid_request_error";

    [JsonPropertyName("param")]
    public string? Param { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
