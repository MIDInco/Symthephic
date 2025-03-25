using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class ReadyUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private float showDuration = 3f;         // 「Ready」表示時間
    [SerializeField] private float delayAfterHide = 1f;       // 表示が消えてからの追加待機時間

    public IEnumerator PlayReadySequence(Action onComplete = null)
    {
        Debug.Log("🟡 Ready演出 開始");

        if (readyText != null)
        {
            readyText.text = "Ready";
            readyText.gameObject.SetActive(true);
            Debug.Log("🟢 Readyテキストを表示しました");
        }
        else
        {
            Debug.LogWarning("⚠ ReadyText が null です！");
        }

        yield return new WaitForSecondsRealtime(showDuration);

        if (readyText != null)
            readyText.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(delayAfterHide);

        onComplete?.Invoke();
    }
}
