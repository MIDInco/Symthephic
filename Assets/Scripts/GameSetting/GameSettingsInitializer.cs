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
        Debug.Log("🔁 GameSettingsInitializer.Awake() 開始");

        GameSettingsData loaded = GameSettingsFileManager.LoadOrCreate();
        Debug.Log($"📘 読み込んだJSON: speed={loaded.NoteSpeed}, volume={loaded.MasterVolume}");

        GameSettings.NoteSpeed = loaded.NoteSpeed;
        GameSettings.MasterVolume = loaded.MasterVolume;
        GameSettings.NoteOffsetValue = loaded.NoteOffsetValue;
        GameSettings.ChartDelay = loaded.ChartDelay;

        Debug.Log("✅ GameSettings に値を反映完了");
    }
    else
    {
        Destroy(gameObject);
    }
}


void Start()
{
    ApplySettings();
}
    else
    {
        Debug.LogWarning("⚠ GameSettingsInitializer: UIController が見つかりませんでした");
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
