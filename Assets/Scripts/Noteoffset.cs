using UnityEngine;

[System.Serializable]
public class Noteoffset : MonoBehaviour
{
    public static Noteoffset Instance;

    public float NoteoffsetValue = 0.0f; // 🎯 デフォルトのオフセット
    private float detectedBPM = 120f; // 🎯 取得した BPM を格納

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

    // 🎯 NotesGenerator から取得した BPM を保存
    public void UpdateBPM(float bpm)
    {
        if (bpm > 0) // 🎯 負の BPM を防ぐ
        {
            detectedBPM = bpm;
            Debug.Log($"[Noteoffset] BPM Updated: {detectedBPM}");
        }
    }

    // 🎯 現在の BPM に応じたオフセットを計算
    public float GetOffset()
    {
        return NoteoffsetValue * (120f / detectedBPM); // 🎯 基準 BPM 120 で補正
    }

    // 🎯 BPM を引数にしたオフセット計算メソッド（新規追加）
    public float GetOffsetForBPM(float bpm)
    {
        return NoteoffsetValue * (120f / bpm);
    }
}
