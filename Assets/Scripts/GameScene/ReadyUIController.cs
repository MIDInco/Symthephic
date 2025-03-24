using UnityEngine;
using TMPro;
using System.Collections;

public class ReadyUIController : MonoBehaviour
{
    public TextMeshProUGUI readyText;
    public float showDuration = 3f;
    public float delayAfterHide = 1f;

public IEnumerator PlayReadySequence()
{
    if (readyText != null)
    {
        readyText.text = "Ready";
        readyText.gameObject.SetActive(true);
    }

    yield return new WaitForSecondsRealtime(showDuration); // Ready表示時間

    if (readyText != null)
        readyText.gameObject.SetActive(false);

    yield return new WaitForSecondsRealtime(delayAfterHide); // 余韻

    // ✅ ここでオーディオと譜面を再開
    if (AudioManager.Instance?.audioSource?.clip != null)
    {
        AudioManager.Instance.audioSource.UnPause();
        Debug.Log("🔊 AudioManager: Ready終了後にUnPauseしました");
    }

    ChartPlaybackManager chart = FindFirstObjectByType<ChartPlaybackManager>();
    chart?.ResumeChart();

    // ✅ 必要なら、GameSceneManager などのフラグ更新もここで
}

}
