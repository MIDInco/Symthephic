using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public int countdownTime = 3;
    public ChartPlaybackManager chartPlaybackManager;

    public static bool IsCountingDown { get; private set; } = false;

    void Start()
    {
        Debug.Log("🔍 CountdownManager: Start() 開始");

        if (countdownText == null)
        {
            Debug.LogError("❌ countdownText が設定されていません！Inspector でアタッチしてください。");
        }
        else
        {
            Debug.Log($"✅ countdownText = {countdownText.name}");
        }

        if (chartPlaybackManager == null)
        {
            Debug.LogError("❌ chartPlaybackManager が null です！");
        }
        else
        {
            Debug.Log($"✅ chartPlaybackManager = {chartPlaybackManager.name}");
        }

        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        Debug.Log("⏳ CountdownRoutine 開始");
        IsCountingDown = true;

        for (int i = countdownTime; i > 0; i--)
        {
            countdownText.text = i.ToString();
            Debug.Log($"⏳ カウントダウン: {i}");
            yield return new WaitForSeconds(1.0f);
        }

        countdownText.text = "Go!";
        Debug.Log("🚀 カウントダウン終了 → ChartPlaybackManager に通知");

        if (chartPlaybackManager != null)
        {
            Debug.Log("✅ ChartPlaybackManager.StartPlayback() を呼び出します！");
            chartPlaybackManager.StartPlayback();
        }
        else
        {
            Debug.LogError("❌ chartPlaybackManager が NULL のため、StartPlayback() を呼べません！");
        }

        yield return new WaitForSeconds(1.0f);
        countdownText.gameObject.SetActive(false);
        IsCountingDown = false;
    }
}
