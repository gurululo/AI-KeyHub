using AI_KeyHub.Models;

namespace AI_KeyHub.Services;

public interface IDataStorage
{
    AppData GetData();
    void SaveData(AppData data);
}
