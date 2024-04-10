using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public interface IAuthService
{
    void EnsureInitialized();
    bool ValidateCredentials(string username, string password, out AdminUser? admin);
}
