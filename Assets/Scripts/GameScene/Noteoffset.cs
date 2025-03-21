using UnityEngine;

[System.Serializable]
public class Noteoffset : MonoBehaviour
{
    public static Noteoffset Instance;
    public float NoteoffsetValue = 0.0f; // ノートのオフセット
    private float detectedBPM = 120f; // 取得した BPM
    public float chartDelay = 0.0f; // 🎯 Chart Delay を追加

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
        return chartDelay; // 🎯 Chart Delay を返す
    }

    // ノート補正値をゲーム内UIなどから変更できるようにする準備
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
