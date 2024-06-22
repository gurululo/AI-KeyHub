using System.Security.Cryptography;
using System.Text;
using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IDataStorage _storage;

    public ApiKeyService(IDataStorage storage)
    {
        _storage = storage;
    }

    public List<InternalApiKey> GetAllApiKeys()
    {
        var data = _storage.GetData();
        return data.ApiKeys.OrderByDescending(k => k.CreatedAt).ToList();
    }

    public InternalApiKey? GetApiKeyById(string id)
    {
        var data = _storage.GetData();
        return data.ApiKeys.FirstOrDefault(k => k.Id == id);
    }

    public (InternalApiKey Entity, string PlainKey) CreateApiKey(string name, string remarks, string channelId)
    {
        // 生成高强度安全随机密钥，格式为 sk-hub-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        byte[] randomBytes = RandomNumberGenerator.GetBytes(24);
        string randomPart = Convert.ToHexString(randomBytes).ToLowerInvariant();
        string plainKey = $"sk-hub-{randomPart}";

        string keyHash = ComputeSha256(plainKey);
        string prefix = plainKey.Length >= 11 ? plainKey[..11] : plainKey;
        string suffix = plainKey.Length >= 4 ? plainKey[^4..] : "";
        string masked = $"{prefix}****{suffix}";

        var entity = new InternalApiKey
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name.Trim(),
            Remarks = remarks?.Trim() ?? string.Empty,
            ChannelId = channelId,
            KeyHash = keyHash,
            KeyPrefix = prefix,
            KeySuffix = suffix,
            MaskedKey = masked,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        var data = _storage.GetData();
        data.ApiKeys.Add(entity);
        _storage.SaveData(data);

        return (entity, plainKey);
    }

    public bool ToggleApiKeyStatus(string id)
    {
        var data = _storage.GetData();
        var existing = data.ApiKeys.FirstOrDefault(k => k.Id == id);
        if (existing != null)
        {
            existing.IsEnabled = !existing.IsEnabled;
            _storage.SaveData(data);
            return existing.IsEnabled;
        }
        return false;
    }

    public void DeleteApiKey(string id)
    {
        var data = _storage.GetData();
        var existing = data.ApiKeys.FirstOrDefault(k => k.Id == id);
        if (existing != null)
        {
            data.ApiKeys.Remove(existing);
            _storage.SaveData(data);
        }
    }

    public InternalApiKey? ValidateAndGetApiKey(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            return null;
        }

        string incomingHash = ComputeSha256(rawKey.Trim());
        var data = _storage.GetData();

        var key = data.ApiKeys.FirstOrDefault(k => 
            string.Equals(k.KeyHash, incomingHash, StringComparison.OrdinalIgnoreCase) && k.IsEnabled);

        return key;
    }

    private static string ComputeSha256(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
