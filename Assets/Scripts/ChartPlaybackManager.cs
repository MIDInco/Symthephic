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
        Debug.Log("🚀 カウントダウン終了！オーディオと譜面を開始");

        StartPlayback();
        yield return new WaitForSeconds(1.0f); // "Go!" の表示時間
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false); // 🎯 カウントダウン UI を非表示
        }
    }

    void StartPlayback()
    {
        // 🎯 正確な時間をセット
        double playbackStartTime = AudioSettings.dspTime;

        // 🎯 オーディオを再生
        AudioManager.Instance.PlayAudioNow();

        // 🎯 譜面の開始時間をセット
        notesGenerator.StartPlayback(); // ✅ `StartPlayback()` に変更

    }
}
