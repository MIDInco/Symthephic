using UnityEngine;
using System.Collections;
public class GameSceneManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    // 🎯 ポーズ補正用の時間管理
    private static double pausedStartTime = 0.0;
    private static double totalPausedDuration = 0.0;

    public static GameSceneManager Instance { get; private set; }

    public ReadyUIController readyUIController; // ← インスペクタでアタッチ

    public static bool IsResuming { get; private set; } = false;

    [SerializeField] private PauseManager pauseManager; // ← インスペクタでアサインする用


void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
    else
    {
        Destroy(gameObject);
    }
}

    void Start()
    {
        Debug.Log("🎯 GameSceneManager: Start() 実行");

        if (SongManager.SelectedSong == null)
        {
            Debug.LogError("❌ GameSceneManager: SongManager.SelectedSong が null です！");
            return;
        }

        Debug.Log($"🎶 選択された曲: {SongManager.SelectedSong.DisplayName} (MIDI: {SongManager.SelectedSong.MidiFileName}, Audio: {SongManager.SelectedSong.AudioFileName})");

        // ✅ オーディオの読み込み
        AudioManager.Instance.PlaySelectedAudio();

        // ✅ MIDI譜面の読み込み
        NotesGenerator generator = FindFirstObjectByType<NotesGenerator>();
        if (generator != null)
        {
            generator.LoadSelectedMidiAndGenerateNotes();
        }
    }

    // 🎯 ポーズ中を考慮した「ゲーム内dspTime」を返す
    public static double GetGameDspTime()
    {
        double now = AudioSettings.dspTime;

        if (IsPaused)
        {
            return pausedStartTime - totalPausedDuration;
        }
        else
        {
            return now - totalPausedDuration;
        }
    }

// static → インスタンスメソッドに変更
    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        pausedStartTime = AudioSettings.dspTime;

        Time.timeScale = 0f;
        AudioManager.Instance?.audioSource.Pause();

        Debug.Log("⏸ ポーズしました");

        // PauseManagerにUI表示を依頼
        if (pauseManager != null)
        {
            pauseManager.Pause();
        }
        else
        {
            Debug.LogWarning("PauseManager が設定されていません！");
        }
    }


    public void ResumeGame()
    {
        if (!IsPaused) return;

        if (Instance != null)
        {
            StartCoroutine(ResumeWithDelay());

            // Resume UI 閉じる（お好みで）
            if (pauseManager != null)
            {
                pauseManager.Resume();
            }
        }
        else
        {
            Debug.LogError("❌ GameSceneManager.Instance が null です。Resume できません！");
        }
    }


private void ResumeNow()
{
    double resumeTime = AudioSettings.dspTime;
    totalPausedDuration += resumeTime - pausedStartTime;

    IsPaused = false;
    Time.timeScale = 1f;

    AudioManager.Instance?.audioSource.UnPause();

    Debug.Log("▶ ResumeNow() によって即時再開しました");
}

private IEnumerator ResumeWithDelay()
{
    IsResuming = true; // ← Ready中フラグON

    Debug.Log("⏳ Readyシーケンス開始");

    if (readyUIController != null)
    {
        yield return StartCoroutine(
            readyUIController.PlayReadySequence(() =>
            {
                double resumeTime = AudioSettings.dspTime;
                totalPausedDuration += resumeTime - pausedStartTime;

                IsPaused = false;
                IsResuming = false; // ← Ready完了後に解除
                Time.timeScale = 1f;

                AudioManager.Instance?.audioSource.UnPause();
                Debug.Log("▶ Ready完了 → ゲーム再開！");
            })
        );
    }
    else
    {
        Debug.LogWarning("⚠ ReadyUIController が未設定。即時再開します");
        ResumeNow(); // フォールバック（あとでこっちにも IsResuming 対応してもOK）
    }
}

}
