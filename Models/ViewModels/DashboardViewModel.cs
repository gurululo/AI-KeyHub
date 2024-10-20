namespace AI_KeyHub.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalChannels { get; set; }
    public int ActiveChannels { get; set; }
    public int TotalApiKeys { get; set; }
    public int ActiveApiKeys { get; set; }

    public List<ChannelViewModel> RecentChannels { get; set; } = new();
    public List<ApiKeyViewModel> RecentApiKeys { get; set; } = new();

    public string ApiEndpointUrl { get; set; } = string.Empty;
}
