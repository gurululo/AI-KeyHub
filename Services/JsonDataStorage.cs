using System.Text.Json;
using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public class JsonDataStorage : IDataStorage
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonDataStorage(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _filePath = Path.Combine(dataDir, "data.json");
    }

    public AppData GetData()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
            {
                return new AppData();
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new AppData();
                }
                var data = JsonSerializer.Deserialize<AppData>(json, JsonOptions);
                return data ?? new AppData();
            }
            catch
            {
                return new AppData();
            }
        }
    }

    public void SaveData(AppData data)
    {
        lock (_lock)
        {
            var tempPath = _filePath + ".tmp";
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }
    }
}
