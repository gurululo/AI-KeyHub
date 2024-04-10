namespace AI_KeyHub.Models;

public class AdminUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
