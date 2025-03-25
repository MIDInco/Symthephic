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

    public void StartPlayback()
    {
        Debug.Log("🎬 ChartPlaybackManager: StartPlayback() が呼ばれました！");

        if (AudioManager.Instance == null || AudioManager.Instance.audioSource == null)
        {
            Debug.LogError("❌ AudioManager または audioSource が NULL のため、オーディオを再生できません！");
            return;
        }

        Debug.Log("✅ オーディオを再生します！");
        AudioManager.Instance.audioSource.Play();

        if (notesGenerator == null)
        {
            Debug.LogError("❌ NotesGenerator が NULL のため、譜面の再生ができません！");
            return;
        }

        Debug.Log("✅ 譜面の再生を開始します！");
        OnAudioPlaybackStarted();
    }

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
}
