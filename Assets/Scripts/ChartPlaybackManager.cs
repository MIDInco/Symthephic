using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChartPlaybackManager : MonoBehaviour
{
    public NotesGenerator notesGenerator; // 🎯 譜面管理クラス
    public Text countdownText; // 🎯 カウントダウン用の UI テキスト
    public int countdownTime = 3; // 🎯 カウントダウン秒数

    void Start()
    {
        Debug.Log("🎯 ChartPlaybackManager の Start が実行されました");

        if (AudioManager.Instance == null)
        {
            Debug.LogError("❌ AudioManager が見つかりません！");
            return;
        }

        if (notesGenerator == null)
        {
            notesGenerator = FindFirstObjectByType<NotesGenerator>(); // 🎯 `NotesGenerator` を取得
            if (notesGenerator == null)
            {
                Debug.LogError("❌ NotesGenerator がシーンに見つかりません！");
                return;
            }
        }

        // 🎯 オーディオの再生開始イベントをリッスン
        AudioManager.Instance.OnAudioPlaybackStarted += OnAudioPlaybackStarted;

        StartCoroutine(CountdownRoutine()); // 🎯 カウントダウン開始
    }

    IEnumerator CountdownRoutine()
    {
        for (int i = countdownTime; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }
            Debug.Log($"⏳ カウントダウン: {i}");
            yield return new WaitForSeconds(1.0f);
        }

        if (countdownText != null)
        {
            countdownText.text = "Go!";
        }
        Debug.Log("🚀 カウントダウン終了！オーディオを再生");

        // 🎯 オーディオを再生 (譜面は `OnAudioPlaybackStarted` で処理)
        AudioManager.Instance.PlayAudioNow();

        yield return new WaitForSeconds(1.0f); // "Go!" の表示時間
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false); // 🎯 カウントダウン UI を非表示
        }
    }

    // 🎯 オーディオの再生が開始されたときに呼ばれる
    void OnAudioPlaybackStarted()
    {
        double audioStartTime = AudioSettings.dspTime; // 🎯 オーディオ再生の正確な時間

        // 🎯 Chart Delay を考慮した譜面の再生開始時間
        float chartDelay = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
        double adjustedStartTime = audioStartTime + chartDelay;

        Debug.Log($"🎯 オーディオの再生が開始: {audioStartTime:F3} sec");
        Debug.Log($"⏳ Chart Delay: {chartDelay} 秒 → 譜面の開始時間: {adjustedStartTime:F3} sec");

        // 🎯 NotesGenerator に再生開始時間を設定
        notesGenerator.SetStartTime(adjustedStartTime);
        notesGenerator.StartPlayback();
    }
}
