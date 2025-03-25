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
    private const string VolumeParameter = "MasterVolume"; // AudioMixer上のパラメータ名

    [Header("判定タイミング補正")]
    public Slider noteOffsetSlider;
    public TextMeshProUGUI noteOffsetLabel;
    private const string NoteOffsetKey = "NoteOffsetValue";

    [Header("チャートディレイ補正")]
    public Slider chartDelaySlider;
    public TextMeshProUGUI chartDelayLabel;
    private const string ChartDelayKey = "ChartDelay";

    public void OnMasterVolumeSliderChanged(float value)
{
    SetVolume(value);
}




    [Header("ノートスピード")]
    public Slider speedSlider;
    public TextMeshProUGUI speedLabel;

    private const string SpeedKey = "NoteSpeed";

    void Start()
    {
        // マスターボリューム設定の復元
    float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
    masterVolumeSlider.value = savedVolume;
    SetVolume(savedVolume);
    masterVolumeSlider.onValueChanged.AddListener(SetVolume);

        // 保存されたスピードを読み込み（なければ10.0をデフォルト）
        float savedSpeed = PlayerPrefs.GetFloat(SpeedKey, 10.0f);
        GameSettings.NoteSpeed = savedSpeed;

        speedSlider.value = savedSpeed;
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        UpdateLabel(savedSpeed);

            // 判定タイミング補正（NoteOffsetValue）の復元
    float savedNoteOffset = PlayerPrefs.GetFloat(NoteOffsetKey, 0.0f);
    noteOffsetSlider.value = savedNoteOffset;
    noteOffsetSlider.onValueChanged.AddListener(OnNoteOffsetChanged);
    noteOffsetLabel.text = $"判定補正: {savedNoteOffset:F3}s";

    // チャートディレイ補正（ChartDelay）の復元
    float savedChartDelay = PlayerPrefs.GetFloat(ChartDelayKey, 0.0f);
    chartDelaySlider.value = savedChartDelay;
    chartDelaySlider.onValueChanged.AddListener(OnChartDelayChanged);
    chartDelayLabel.text = $"チャート遅延: {savedChartDelay:F3}s";

    // Noteoffset に値を反映（あれば）
    if (Noteoffset.Instance != null)
    {
        Noteoffset.Instance.SetNoteOffsetValue(savedNoteOffset);
        Noteoffset.Instance.chartDelay = savedChartDelay;
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
    }

    void OnSpeedChanged(float value)
    {
        GameSettings.NoteSpeed = value;
        PlayerPrefs.SetFloat(SpeedKey, value); // 保存
        PlayerPrefs.Save();

        UpdateLabel(value);
        Debug.Log($"🎚 ノートスピードが変更され、保存されました: {value:0.0}x");
    }

    void UpdateLabel(float value)
    {
        if (speedLabel != null)
            speedLabel.text = $"現在のスピード: {value:0.0}x";
    }

/// <summary>
/// オーディオの音量を設定し、関連する設定を更新します。
/// </summary>
/// <param name="value">
/// 0.0から1.0の範囲で正規化された音量レベル。
/// </param>
/// <remarks>
/// このメソッドは、デシベルの対数スケールを使用してオーディオミキサーの音量を調整し、
/// ゲーム設定のマスターボリュームを更新し、その値をPlayerPrefsに保存します。
/// また、UI内の音量ラベルを更新し、デバッグ用に音量設定をログに記録します。
/// </remarks>
public void SetVolume(float value)
{
    float minVolume = 0.0001f;
    float volume = Mathf.Lerp(minVolume, 1f, value);
    float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(value, 0.3f));
    audioMixer.SetFloat(VolumeParameter, db);

    GameSettings.MasterVolume = value;
    PlayerPrefs.SetFloat(VolumeKey, value);    // ←★追加！
    PlayerPrefs.Save();                        // ←★追加！

    UpdateVolumeLabel(value);
    Debug.Log($"🔊 ボリューム設定: value={value}, dB={db}");
}


void UpdateVolumeLabel(float value)
{
    if (volumeLabel != null)
    {
        volumeLabel.text = $"全体音量：{(value * 100f):0}%";
    }
}

void OnNoteOffsetChanged(float value)
{
    PlayerPrefs.SetFloat(NoteOffsetKey, value);
    PlayerPrefs.Save();
    noteOffsetLabel.text = $"判定補正: {value:F3}s";

    if (Noteoffset.Instance != null)
        Noteoffset.Instance.SetNoteOffsetValue(value);
}

void OnChartDelayChanged(float value)
{
    PlayerPrefs.SetFloat(ChartDelayKey, value);
    PlayerPrefs.Save();
    chartDelayLabel.text = $"チャート遅延: {value:F3}s";

    if (Noteoffset.Instance != null)
        Noteoffset.Instance.chartDelay = value;
}


//~~~~~~~~~~~~~~~リセットボタンはなるべく最後に奥！~~~~~~~~~~~~~~~~~~~~~~//

public void ResetSettings()
{
    // 初期値を定義
    float defaultSpeed = 10.0f;
    float defaultVolume = 1f;

    // スライダーの値を変更（これでSetVolume/OnSpeedChangedが呼ばれる）
    speedSlider.value = defaultSpeed;
    masterVolumeSlider.value = defaultVolume;

    // 保存（※SetVolume/OnSpeedChangedの中で保存されるのでここでは不要）

    Debug.Log("🔁 設定をリセットしました");

    float defaultNoteOffset = 0.0f;
    float defaultChartDelay = 0.1f;

    // スライダー更新（ハンドラが自動的に呼ばれる）
    noteOffsetSlider.value = defaultNoteOffset;
    chartDelaySlider.value = defaultChartDelay;
}

}
