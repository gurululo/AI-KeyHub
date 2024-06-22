using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public interface IApiKeyService
{
    List<InternalApiKey> GetAllApiKeys();
    InternalApiKey? GetApiKeyById(string id);
    (InternalApiKey Entity, string PlainKey) CreateApiKey(string name, string remarks, string channelId);
    bool ToggleApiKeyStatus(string id);
    void DeleteApiKey(string id);
    InternalApiKey? ValidateAndGetApiKey(string rawKey);
}
