using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class GameSettingsUIController : MonoBehaviour
{
    public GameObject settingPanel;

    [Header("マスターボリューム")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public TextMeshProUGUI volumeLabel;
    private const string VolumeKey = "MasterVolume";
    private const string VolumeParameter = "MasterVolume";

    [Header("ノートスピード")]
    public Slider speedSlider;
    public TextMeshProUGUI speedLabel;
    private const string SpeedKey = "NoteSpeed";

    [Header("判定タイミング補正")]
    public Slider noteOffsetSlider;
    public TextMeshProUGUI noteOffsetLabel;
    private const string NoteOffsetKey = "NoteOffsetValue";

    [Header("チャートディレイ補正")]
    public Slider chartDelaySlider;
    public TextMeshProUGUI chartDelayLabel;
    private const string ChartDelayKey = "ChartDelay";

    void Start()
    {
        // イベント登録
        masterVolumeSlider.onValueChanged.AddListener(UpdateVolumeLabel);
        masterVolumeSlider.onValueChanged.AddListener(SetVolume);

        speedSlider.onValueChanged.AddListener(UpdateLabel);
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);

        noteOffsetSlider.onValueChanged.AddListener(OnNoteOffsetChanged);
        chartDelaySlider.onValueChanged.AddListener(OnChartDelayChanged);

        // GameSettings の値を反映
        masterVolumeSlider.value = GameSettings.MasterVolume;
        speedSlider.value = GameSettings.NoteSpeed;
        noteOffsetSlider.value = GameSettings.NoteOffsetValue;
        chartDelaySlider.value = GameSettings.ChartDelay;

        UpdateVolumeLabel(GameSettings.MasterVolume);
        UpdateLabel(GameSettings.NoteSpeed);
        OnNoteOffsetChanged(GameSettings.NoteOffsetValue);
        OnChartDelayChanged(GameSettings.ChartDelay);

        if (Noteoffset.Instance != null)
        {
            Noteoffset.Instance.SetNoteOffsetValue(GameSettings.NoteOffsetValue);
            Noteoffset.Instance.chartDelay = GameSettings.ChartDelay;
        }
    }

    public void OpenPanel()
    {
        Debug.Log("✅ OpenPanel() が呼び出されました");
        if (settingPanel != null)
            settingPanel.SetActive(true);
        else
            Debug.LogError("❌ settingPanel が null です！");
    }
public void ClosePanel()
{
    Debug.Log("❎ ClosePanel() が呼び出されました");
    settingPanel.SetActive(false);

    // GameSettings に保存
    GameSettings.NoteSpeed = speedSlider.value;
    GameSettings.MasterVolume = masterVolumeSlider.value;
    GameSettings.NoteOffsetValue = noteOffsetSlider.value;
    GameSettings.ChartDelay = chartDelaySlider.value;

    SaveToJson(); // jsonに保存

    Debug.Log("💾 設定をJSONに保存しました");
}


    public void SetVolume(float value)
    {
        float minVolume = 0.0001f;
        float volume = Mathf.Lerp(minVolume, 1f, value);
        float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(value, 0.3f));
        audioMixer.SetFloat(VolumeParameter, db);

        GameSettings.MasterVolume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();

        UpdateVolumeLabel(value);
        Debug.Log($"🔊 ボリューム設定: value={value}, dB={db}");
    }

    void UpdateVolumeLabel(float value)
    {
        volumeLabel.text = $"全体音量: {(value * 100f):0}%";
    }

    void OnSpeedChanged(float value)
    {
        GameSettings.NoteSpeed = value;
        PlayerPrefs.SetFloat(SpeedKey, value);
        PlayerPrefs.Save();

        UpdateLabel(value);
        Debug.Log($"🎚 ノートスピード: {value:0.0}x");
    }

    void UpdateLabel(float value)
    {
        speedLabel.text = $"ノートスピード: {value:0.0}x";
    }

    void OnNoteOffsetChanged(float value)
    {
        noteOffsetLabel.text = $"判定タイミング補正: {(value >= 0 ? "+" : "")}{value:F3}s";

        GameSettings.NoteOffsetValue = value;
        PlayerPrefs.SetFloat(NoteOffsetKey, value);
        PlayerPrefs.Save();

        if (Noteoffset.Instance != null)
            Noteoffset.Instance.SetNoteOffsetValue(value);
    }

    void OnChartDelayChanged(float value)
    {
        chartDelayLabel.text = $"チャート遅延補正: {(value >= 0 ? "+" : "")}{value:F3}s";

        GameSettings.ChartDelay = value;
        PlayerPrefs.SetFloat(ChartDelayKey, value);
        PlayerPrefs.Save();

        if (Noteoffset.Instance != null)
            Noteoffset.Instance.chartDelay = value;
    }

    public void ApplySettingsToUI()
    {
        Debug.Log("📥 ApplySettingsToUI: GameSettings の値を UI に反映します");
        Debug.Log($"    GameSettings.NoteSpeed = {GameSettings.NoteSpeed}");
        Debug.Log($"    GameSettings.NoteOffsetValue = {GameSettings.NoteOffsetValue}");
        Debug.Log($"    GameSettings.ChartDelay = {GameSettings.ChartDelay}");

        masterVolumeSlider.value = GameSettings.MasterVolume;
        speedSlider.value = GameSettings.NoteSpeed;
        noteOffsetSlider.value = GameSettings.NoteOffsetValue;
        chartDelaySlider.value = GameSettings.ChartDelay;

        UpdateVolumeLabel(GameSettings.MasterVolume);
        UpdateLabel(GameSettings.NoteSpeed);
        OnNoteOffsetChanged(GameSettings.NoteOffsetValue);
        OnChartDelayChanged(GameSettings.ChartDelay);
    }

    public void ResetSettings()
    {
        float defaultSpeed = 10.0f;
        float defaultVolume = 1f;
        float defaultNoteOffset = 0.0f;
        float defaultChartDelay = 0.2f;

        speedSlider.value = defaultSpeed;
        masterVolumeSlider.value = defaultVolume;
        noteOffsetSlider.value = defaultNoteOffset;
        chartDelaySlider.value = defaultChartDelay;

        Debug.Log("🔁 設定をリセットしました");
    }

    public void SaveToJson()
{
    var data = new GameSettingsData()
    {
        NoteSpeed = GameSettings.NoteSpeed,
        MasterVolume = GameSettings.MasterVolume,
        NoteOffsetValue = GameSettings.NoteOffsetValue,
        ChartDelay = GameSettings.ChartDelay
    };
    GameSettingsFileManager.Save(data);
}

}
