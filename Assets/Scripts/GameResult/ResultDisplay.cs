using UnityEngine;
using TMPro;
//テスト用

public class ResultDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI phraseText;

void Start()
{
    Debug.Log("🧪 [ResultDisplay] Start() 実行");

    float score = GameResultData.ScorePercent;
    int phrase = GameResultData.MaxPhrase;

    Debug.Log($"📊 [ResultDisplay] GameResultData → Score={score}, MaxPhrase=x{phrase}");

    if (scoreText != null)
        scoreText.text = $"Score: {score:0.0}%";
    else
        Debug.LogWarning("⚠ scoreText が設定されていません");

    if (phraseText != null)
        phraseText.text = $"Max Phrase: x{phrase}";
    else
        Debug.LogWarning("⚠ phraseText が設定されていません");
}
}
