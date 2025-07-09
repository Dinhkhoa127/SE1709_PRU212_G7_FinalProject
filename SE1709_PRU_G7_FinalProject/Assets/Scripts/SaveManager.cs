using UnityEngine;

/// <summary>
/// Quản lý việc lưu và tải dữ liệu Player bằng PlayerPrefs (dùng được cho cả WebGL, PC...).
/// </summary>
public static class SaveManager
{
    public static void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("playerData", json);
        PlayerPrefs.Save();
    }

    public static PlayerData Load()
    {
        if (PlayerPrefs.HasKey("playerData"))
        {
            string json = PlayerPrefs.GetString("playerData");
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return null;
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey("playerData");
    }
}
