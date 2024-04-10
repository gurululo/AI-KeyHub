namespace AI_KeyHub.Models;

public class InternalApiKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string KeySuffix { get; set; } = string.Empty;
    public string MaskedKey { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
