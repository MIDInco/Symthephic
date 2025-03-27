using UnityEngine;

public class ChartPlaybackManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;

    void Awake()
    {
        Debug.Log($"🔍 ChartPlaybackManager: Awake() 実行 (GameObject: {gameObject.name})");
    }

    void Start()
    {
        Debug.Log($"✅ ChartPlaybackManager: Start() 実行 (GameObject: {gameObject.name})");

        if (notesGenerator == null)
        {
            Debug.LogError("❌ ChartPlaybackManager: NotesGenerator が設定されていません！");
        }
        else
        {
            Debug.Log($"✅ NotesGenerator が正しく設定されています: {notesGenerator.gameObject.name}");
        }
    }

    private System.Collections.IEnumerator DelayStartPlaybackCoroutine()
{
    yield return null; // 1フレーム待機して確実に再生が始まった後に取得

    if (notesGenerator == null)
    {
        Debug.LogError("❌ NotesGenerator が NULL のため、譜面の再生ができません！");
        yield break;
    }

    double audioStartTime = AudioSettings.dspTime;
    float chartDelay = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
    double adjustedStartTime = audioStartTime + chartDelay;

    Debug.Log($"🎯 オーディオの再生が確認された: {audioStartTime:F6} 秒");
    Debug.Log($"⏳ Chart Delay: {chartDelay} 秒 → 譜面の開始時間: {adjustedStartTime:F6} 秒");

    notesGenerator.SetStartTime(adjustedStartTime);
    notesGenerator.StartPlayback();
}

public void StartPlayback()
{
    Debug.Log("🎬 ChartPlaybackManager: StartPlayback() が呼ばれました！");

    if (AudioManager.Instance == null || AudioManager.Instance.audioSource == null)
    {
        Debug.LogError("❌ AudioManager または audioSource が NULL のため、オーディオを再生できません！");
        return;
    }

    AudioManager.Instance.audioSource.Play();
    StartCoroutine(DelayStartPlaybackCoroutine());
}

// ✅ この関数全体を削除またはコメントアウトする
/*
private void OnAudioPlaybackStarted()
{
    double audioStartTime = AudioSettings.dspTime;
    float chartDelay = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
    double adjustedStartTime = audioStartTime + chartDelay;

    Debug.Log($"🎯 オーディオの再生が開始: {audioStartTime:F3} sec");
    Debug.Log($"⏳ Chart Delay: {chartDelay} 秒 → 譜面の開始時間: {adjustedStartTime:F3} sec");

    notesGenerator.SetStartTime(adjustedStartTime);
    notesGenerator.StartPlayback();
}
*/

    public void PauseChart()
    {
        notesGenerator?.PausePlayback();
    }

    public void ResumeChart()
    {
        if (AudioManager.Instance?.audioSource?.clip != null)
        {
            AudioManager.Instance.audioSource.UnPause();
            Debug.Log("🔊 AudioManager: ResumeChart で UnPause しました");
        }

        notesGenerator?.ResumePlayback();
    }

    // 🎼 ノーツだけ生成（再生しない）
public void LoadChartOnly()
{
    if (notesGenerator != null)
    {
        Debug.Log("🎼 ChartPlaybackManager: LoadChartOnly() → ノートを読み込みます");
        notesGenerator.LoadSelectedMidiAndGenerateNotes(); // ノート生成のみ
    }
    else
    {
        Debug.LogError("❌ notesGenerator が null のため、譜面を生成できません");
    }
}

}
