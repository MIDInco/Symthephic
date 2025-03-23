using UnityEngine;
using TMPro;
using System.Collections;

public class ReadyUIController : MonoBehaviour
{
    public TextMeshProUGUI readyText;
    public float showDuration = 3f;
    public float delayAfterHide = 1f;

    public IEnumerator PlayReadySequence(System.Action onComplete)
    {
        // Ready 表示
        if (readyText != null)
        {
            readyText.text = "Ready"; // フォントやスタイルはインスペクタで
            readyText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(showDuration); // 表示時間

        if (readyText != null)
            readyText.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(delayAfterHide); // 表示終了後の余韻時間

        onComplete?.Invoke(); // 完了時のコールバック（再開処理を呼ぶ）
    }
}
