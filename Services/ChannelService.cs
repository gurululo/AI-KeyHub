using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public class ChannelService : IChannelService
{
    private readonly IDataStorage _storage;

    public ChannelService(IDataStorage storage)
    {
        _storage = storage;
    }

    public List<Channel> GetAllChannels()
    {
        var data = _storage.GetData();
        return data.Channels.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Channel? GetChannelById(string id)
    {
        var data = _storage.GetData();
        return data.Channels.FirstOrDefault(c => c.Id == id);
    }

    public void AddChannel(Channel channel)
    {
        var data = _storage.GetData();
        channel.Id = Guid.NewGuid().ToString("N");
        channel.CreatedAt = DateTime.UtcNow;
        data.Channels.Add(channel);
        _storage.SaveData(data);
    }

    public void UpdateChannel(Channel channel, bool keepExistingApiKeyIfEmpty = true)
    {
        var data = _storage.GetData();
        var existing = data.Channels.FirstOrDefault(c => c.Id == channel.Id);
        if (existing != null)
        {
            existing.Name = channel.Name;
            existing.BaseUrl = channel.BaseUrl.TrimEnd('/');
            existing.Model = channel.Model;
            existing.IsEnabled = channel.IsEnabled;
            existing.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(channel.ApiKey) || !keepExistingApiKeyIfEmpty)
            {
                existing.ApiKey = channel.ApiKey;
            }

            _storage.SaveData(data);
        }
    }

    public void DeleteChannel(string id)
    {
        var data = _storage.GetData();
        var existing = data.Channels.FirstOrDefault(c => c.Id == id);
        if (existing != null)
        {
            data.Channels.Remove(existing);
            _storage.SaveData(data);
        }
    }

    public bool ToggleChannelStatus(string id)
    {
        var data = _storage.GetData();
        var existing = data.Channels.FirstOrDefault(c => c.Id == id);
        if (existing != null)
        {
            existing.IsEnabled = !existing.IsEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
            _storage.SaveData(data);
            return existing.IsEnabled;
        }
        return false;
    }

    public string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "******";
        }

        var trimmed = apiKey.Trim();
        if (trimmed.Length <= 8)
        {
            return "********";
        }

        return $"{trimmed[..3]}****{trimmed[^4..]}";
    }
}
