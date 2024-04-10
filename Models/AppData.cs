namespace AI_KeyHub.Models;

public class AppData
{
    public AdminUser? Admin { get; set; }
    public List<Channel> Channels { get; set; } = new();
    public List<InternalApiKey> ApiKeys { get; set; } = new();
}
