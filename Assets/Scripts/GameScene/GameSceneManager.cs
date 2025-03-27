using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
public class GameSceneManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    private static double pausedStartTime = 0.0;
    private static double totalPausedDuration = 0.0;

    public static GameSceneManager Instance { get; private set; }

    public ReadyUIController readyUIController;

    public static bool IsResuming { get; private set; } = false;

    public AudioMixer audioMixer;

    [SerializeField] private PauseManager pauseManager;

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

        IsPaused = false;
        IsResuming = false;
        totalPausedDuration = 0.0;
        pausedStartTime = 0.0;

        Debug.Log("🔁 GameSceneManager: Awake() でポーズ状態をリセットしました");
    }

void Start()
{
    // 1. 設定ファイルから読み込んで GameSettings に適用
    GameSettingsData data = GameSettingsFileManager.LoadOrCreate();
    GameSettings.ApplyFromData(data);

    // 2. AudioMixerへの反映などをまとめて行う (MasterVolumeなど)
    GameSettingsInitializer.Instance?.ApplySettings();

    // AudioManagerが無ければ生成
    if (AudioManager.Instance == null)
    {
        GameObject prefab = Resources.Load<GameObject>("GameScenes/AudioManager");
        if (prefab != null)
        {
            Instantiate(prefab);
            Debug.Log("🎮 AudioManager を GameScene で生成しました！");
        }
        else
        {
            Debug.LogError("❌ AudioManager プレハブが Resources/GameScenes に見つかりません！");
        }
    }

    // Audio再生開始
    AudioManager.Instance?.PlaySelectedAudio();

    // 🆕 譜面だけ先にロード（再生はしない）
    ChartPlaybackManager manager = FindAnyObjectByType<ChartPlaybackManager>();
    if (manager != null)
    {
        manager.LoadChartOnly();
    }
}


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

    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        pausedStartTime = AudioSettings.dspTime;

        Time.timeScale = 0f;
        AudioManager.Instance?.audioSource.Pause();

        Debug.Log("⏸ ポーズしました");

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
        StartCoroutine(ResumeWithDelay()); // ✅ これだけでOK（ReadyUIControllerで3+1秒の待機が入る）
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
        IsResuming = true;
        Debug.Log("⏳ Readyシーケンス開始");

        if (readyUIController != null)
        {
            yield return StartCoroutine(readyUIController.PlayReadySequence(() =>
            {
                Debug.Log("⏱ Ready完了 → Resume処理を実行");

                AudioManager.Instance?.audioSource?.UnPause();
                FindAnyObjectByType<ChartPlaybackManager>()?.ResumeChart();

                double resumeTime = AudioSettings.dspTime;
                NotifyResumed(resumeTime);

                IsPaused = false;
                IsResuming = false;
                Time.timeScale = 1f;
            }));
        }
        else
        {
            Debug.LogWarning("⚠ ReadyUIController が未設定。即時Resumeに切り替えます");
            ResumeNow();
        }
    }

    public static void NotifyResumed(double resumeTime)
    {
        totalPausedDuration += resumeTime - pausedStartTime;
        IsPaused = false;
        IsResuming = false;
        Debug.Log("✅ GameSceneManager: NotifyResumed() でポーズ解除を確定しました");
    }

public void EndGameAndTransitionToResult()
{
    float score = ScoreManager.Instance.GetCurrentScorePercent();
    int phrase = PhraseManager.Instance.GetMaxPhrase();

    Debug.Log($"🧪 [GameSceneManager] スコア取得: {score:0.0}%, フレーズ: x{phrase}");

    GameResultData.ScorePercent = score;
    GameResultData.MaxPhrase = phrase;

    Debug.Log($"✅ [GameSceneManager] 保存完了: Score={GameResultData.ScorePercent}, Phrase={GameResultData.MaxPhrase}");

    SceneTransitionManager transitionManager = FindAnyObjectByType<SceneTransitionManager>();
    if (transitionManager != null)
    {
        transitionManager.LoadResultScene();
    }
}

}
