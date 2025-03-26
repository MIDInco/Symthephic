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
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            GameSettingsData data = JsonUtility.FromJson<GameSettingsData>(json);
            Debug.Log($"📂 設定を読み込み: {FilePath}");
            return data;
        }
        else
        {
            var defaultData = new GameSettingsData();
            Save(defaultData); // ファイルを自動生成
            Debug.Log($"🆕 settings.json を自動生成しました: {FilePath}");
            return defaultData;
        }
    }
}
