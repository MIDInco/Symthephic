using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChartPlaybackManager : MonoBehaviour
{
    public NotesGenerator notesGenerator;
    public Text countdownText;
    public int countdownTime = 3;

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
            notesGenerator = FindFirstObjectByType<NotesGenerator>();
            if (notesGenerator == null)
            {
                Debug.LogError("❌ NotesGenerator がシーンに見つかりません！");
                return;
            }
        }

        AudioManager.Instance.OnAudioPlaybackStarted += OnAudioPlaybackStarted;

        StartCoroutine(CountdownRoutine());
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

        AudioManager.Instance.audioSource.Play();
        
        yield return new WaitForSeconds(1.0f);
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    void OnAudioPlaybackStarted()
    {
        double audioStartTime = AudioSettings.dspTime;

        float chartDelay = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
        double adjustedStartTime = audioStartTime + chartDelay;

        Debug.Log($"🎯 オーディオの再生が開始: {audioStartTime:F3} sec");
        Debug.Log($"⏳ Chart Delay: {chartDelay} 秒 → 譜面の開始時間: {adjustedStartTime:F3} sec");

        notesGenerator.SetStartTime(adjustedStartTime);
        notesGenerator.StartPlayback();
    }
}
