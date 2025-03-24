using UnityEngine;
using UnityEngine.Audio;

public class GameSettingsInitializer : MonoBehaviour
{
    public static GameSettingsInitializer Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer; // Inspector で MasterMixer をアサイン

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameSettingsLoader.Load(audioMixer); // 🔁 一度だけ反映
            Debug.Log("✅ GameSettingsInitializer: 設定を初期化しました");
        }
        else
        {
            Destroy(gameObject); // 🎯 二重生成防止
        }
    }

    public void ApplySettings()
{
    if (audioMixer != null)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(GameSettings.MasterVolume, 0.3f));
        audioMixer.SetFloat("MasterVolume", db);
        Debug.Log($"🔁 GameSettingsInitializer: 再適用 - MasterVolume={GameSettings.MasterVolume} → dB={db}");
    }
}
}
