using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public interface IChannelService
{
    List<Channel> GetAllChannels();
    Channel? GetChannelById(string id);
    void AddChannel(Channel channel);
    void UpdateChannel(Channel channel, bool keepExistingApiKeyIfEmpty = true);
    void DeleteChannel(string id);
    bool ToggleChannelStatus(string id);
    string MaskApiKey(string apiKey);
}
