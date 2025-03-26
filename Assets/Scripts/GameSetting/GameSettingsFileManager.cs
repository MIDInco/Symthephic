using UnityEngine;
using System.IO;

public static class GameSettingsFileManager
{
    private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "settings.json");

    public static void Save(GameSettingsData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"💾 設定を保存: {FilePath}");
    }

    // 🚀 起動時に使う「読み込み or 自動生成」メソッド
public static GameSettingsData LoadOrCreate()
{
    Debug.Log("📥 LoadOrCreate() が呼び出されました");

    if (File.Exists(FilePath))
    {
        string json = File.ReadAllText(FilePath);
        Debug.Log($"📖 settings.json の内容: {json}");

        GameSettingsData data = JsonUtility.FromJson<GameSettingsData>(json);
        Debug.Log("✅ JsonUtility でパース完了");

        return data;
    }
    else
    {
        Debug.Log("❗settings.json が存在しません、初期値で作成します");

        var defaultData = new GameSettingsData();
        Save(defaultData);
        return defaultData;
    }
}
}
