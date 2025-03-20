using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour
{
    public Text countdownText; // UI のカウントダウン表示用
    public int countdownTime = 3; // カウントダウン秒数
    public NotesGenerator notesGenerator;
    private bool countdownFinished = false;

void Start()
{
    if (notesGenerator == null)
    {
        notesGenerator = FindFirstObjectByType<NotesGenerator>(); // ✅ 修正
        if (notesGenerator == null)
        {
            Debug.LogError("❌ NotesGenerator がシーンに見つかりません！");
            return;
        }
    }

    StartCoroutine(CountdownRoutine());
}



    IEnumerator CountdownRoutine()
    {
        for (int i = countdownTime; i > 0; i--)
        {
            countdownText.text = i.ToString();
            Debug.Log($"⏳ カウントダウン: {i}");
            yield return new WaitForSeconds(1.0f);
        }

        countdownText.text = "Go!";
        Debug.Log("🚀 カウントダウン終了！オーディオと譜面を開始");

        StartPlayback();
        yield return new WaitForSeconds(1.0f); // "Go!" の表示時間
        countdownText.gameObject.SetActive(false); // カウントダウン表示を消す
    }

void StartPlayback()
{
    if (!countdownFinished)
    {
        countdownFinished = true;

        // 🎯 正確な時間をセット
        double playbackStartTime = AudioSettings.dspTime;

        // 🎯 オーディオを再生
        AudioManager.Instance.PlayAudioNow();

        // 🎯 譜面の開始時間をセット
        notesGenerator.StartPlayback(); // ✅ `StartPlayback()` に変更
    }
}
}
