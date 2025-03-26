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

        Debug.Log("🧪 GameSettings 値確認");
        Debug.Log($"    NoteSpeed: {GameSettings.NoteSpeed}");
        Debug.Log($"    MasterVolume: {GameSettings.MasterVolume}");
        Debug.Log($"    NoteOffsetValue: {GameSettings.NoteOffsetValue}");
        Debug.Log($"    ChartDelay: {GameSettings.ChartDelay}");

        ApplySettingsToUI();
    }


    

    public void OpenPanel()
    {
        Debug.Log("✅ OpenPanel() が呼び出されました - 開始");

        try
        {
            // .json を読み込み、反映＋ログ出力
            var data = GameSettingsFileManager.LoadOrCreate();
            if (data == null)
            {
                Debug.LogError("❌ LoadOrCreate() の戻り値が null です");
                return;
            }

            Debug.Log("📂 OpenPanel内でsettings.json読み込み成功");
            Debug.Log($"    [JSON] NoteSpeed: {data.NoteSpeed}");
            Debug.Log($"    [JSON] MasterVolume: {data.MasterVolume}");
            Debug.Log($"    [JSON] NoteOffsetValue: {data.NoteOffsetValue}");
            Debug.Log($"    [JSON] ChartDelay: {data.ChartDelay}");

            GameSettings.ApplyFromData(data);
            ApplySettingsToUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ OpenPanel() 中に例外が発生しました: {ex.Message}\n{ex.StackTrace}");
        }

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

    if (audioMixer != null)
    {
        audioMixer.SetFloat(VolumeParameter, db);
    }
    else
    {
        Debug.LogWarning("⚠ SetVolume: audioMixer が未設定です！");
    }

    GameSettings.MasterVolume = value;
    UpdateVolumeLabel(value);
    Debug.Log($"🔊 ボリューム設定: value={value}, dB={db}");
}

    void UpdateVolumeLabel(float value)
    {
        volumeLabel.text = $"全体音量: {(value * 100f):0}%";
    }

    void OnSpeedChanged(float value)
    {
        GameSettings.NoteSpeed = value;UpdateLabel(value);
        Debug.Log($"🎚 ノートスピード: {value:0.0}x");
    }

    void UpdateLabel(float value)
    {
        speedLabel.text = $"ノートスピード: {value:0.0}x";
    }

    void OnNoteOffsetChanged(float value)
    {
        noteOffsetLabel.text = $"判定タイミング補正: {(value >= 0 ? "+" : "")}{value:F3}s";

        GameSettings.NoteOffsetValue = value;if (Noteoffset.Instance != null)
            Noteoffset.Instance.SetNoteOffsetValue(value);
    }

    void OnChartDelayChanged(float value)
    {
        chartDelayLabel.text = $"チャート遅延補正: {(value >= 0 ? "+" : "")}{value:F3}s";

        GameSettings.ChartDelay = value;if (Noteoffset.Instance != null)
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


    public void ReloadSettingsFromJson()
    {
        var data = GameSettingsFileManager.LoadOrCreate();
        GameSettings.ApplyFromData(data);
        ApplySettingsToUI();
        Debug.Log("🔄 ReloadSettingsFromJson: 設定を再読み込みしてUIに反映しました");
    }
}
