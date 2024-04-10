using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public class AuthService : IAuthService
{
    private readonly IDataStorage _storage;
    private readonly IPasswordHasher _hasher;

    public AuthService(IDataStorage storage, IPasswordHasher hasher)
    {
        _storage = storage;
        _hasher = hasher;
    }

    public void EnsureInitialized()
    {
        var data = _storage.GetData();
        if (data.Admin == null)
        {
            // 初始化默认管理员账号：admin / admin123（加盐哈希存储）
            var (hash, salt) = _hasher.HashPassword("admin123");
            data.Admin = new AdminUser
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = "admin",
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };
            _storage.SaveData(data);
        }
    }

    public bool ValidateCredentials(string username, string password, out AdminUser? admin)
    {
        admin = null;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var data = _storage.GetData();
        if (data.Admin == null)
        {
            return false;
        }

        if (string.Equals(data.Admin.Username, username, StringComparison.OrdinalIgnoreCase))
        {
            if (_hasher.VerifyPassword(password, data.Admin.PasswordHash, data.Admin.PasswordSalt))
            {
                admin = data.Admin;
                return true;
            }
        }

        return false;
    }
}
