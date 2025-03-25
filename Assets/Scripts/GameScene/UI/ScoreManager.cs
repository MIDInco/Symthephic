using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText; // Full Combo / All Perfect など

    [Header("設定")]
    public int totalNotes = 0; // 開始時にセットする
    public float goodRatio = 0.5f; // Good = Perfectの何割

    private float currentScorePercent = 0f;
    private int perfectCount = 0;
    private int goodCount = 0;
    private int missCount = 0;

    private int maxPhrase = 0;
    

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetTotalNotes(int total)
    {
        totalNotes = total;
        currentScorePercent = 0f;
        perfectCount = goodCount = missCount = 0;
        UpdateScoreDisplay();
    }

    public void RegisterPerfect()
    {
        perfectCount++;
        AddScore(1f);
    }

    public void RegisterGood()
    {
        goodCount++;
        AddScore(goodRatio);
    }

    public void RegisterMiss()
    {
        missCount++;
        // 加点なし
    }

    private void AddScore(float ratio)
    {
        if (totalNotes == 0) return;

        float perNote = 100f / totalNotes;
        currentScorePercent += perNote * ratio;

        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"{currentScorePercent:0.0}%";
    }

    public void ShowResult()
    {
        if (resultText == null) return;

        if (missCount == 0 && goodCount == 0)
            resultText.text = "ALL PERFECT!";
        else if (missCount == 0)
            resultText.text = "FULL COMBO!";
        else
            resultText.text = "";
    }

    public void ResetScore()
    {
        currentScorePercent = 0f;
        perfectCount = 0;
        goodCount = 0;
        missCount = 0;

        if (scoreText != null)
            scoreText.text = "0.0%";

        if (resultText != null)
            resultText.text = "";
    }
    public float GetCurrentScorePercent()
{
    return currentScorePercent;
}
}
