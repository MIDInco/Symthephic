using UnityEngine;

[System.Serializable]
public class Noteoffset : MonoBehaviour
{
    public static Noteoffset Instance;

    [Tooltip("ノート判定のタイミング補正（秒単位）")]
    [Range(-0.2f, 0.2f)]
    public float NoteoffsetValue = 0.0f; // ノートのオフセット

    private float detectedBPM = 120f; // 取得した BPM

    [Tooltip("譜面再生を遅らせる補正時間（秒）")]
    public float chartDelay = 0.0f; // Chart Delay

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

private void Start()
{
    // PlayerPrefs廃止 → GameSettingsから読み込み
    SetNoteOffsetValue(GameSettings.NoteOffsetValue);
    chartDelay = GameSettings.ChartDelay;

    Debug.Log($"✅ Noteoffset.cs 初期化（.json経由）: Offset={NoteoffsetValue}, Delay={chartDelay}");
}


    public void UpdateBPM(float bpm)
    {
        if (bpm > 0)
        {
            detectedBPM = bpm;
            Debug.Log($"[Noteoffset] BPM Updated: {detectedBPM}");
        }
    }

    public float GetOffset()
    {
        return NoteoffsetValue * (120f / detectedBPM);
    }

    public float GetOffsetForBPM(float bpm)
    {
        return NoteoffsetValue * (120f / bpm);
    }

    public float GetChartDelay()
    {
        return chartDelay;
    }

    // 🎛 ゲーム内UIから変更できるようにする準備
    public void SetNoteOffsetValue(float value)
    {
        NoteoffsetValue = value;
        Debug.Log($"🎛 ノートオフセットが更新されました: {NoteoffsetValue}");
    }

    public float GetNoteOffsetValue()
    {
        return NoteoffsetValue;
    }
}
