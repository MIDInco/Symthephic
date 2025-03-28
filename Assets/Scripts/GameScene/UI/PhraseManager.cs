using UnityEngine;
using TMPro;

public class PhraseManager : MonoBehaviour
{
    public static PhraseManager Instance { get; private set; }

    public TextMeshProUGUI phraseText; // UI表示用

    private int currentPhrase = 0;
    private int maxPhrase = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 成功時に呼び出し：Phrase加算
    /// </summary>
    public void IncrementPhrase()
    {
        currentPhrase++;
        if (currentPhrase > maxPhrase)
            maxPhrase = currentPhrase;

        UpdatePhraseDisplay();
    }

    /// <summary>
    /// 成功時に呼び出し：Phrase加算（JudgmentManager旧コード互換）
    /// </summary>
    public void IncreasePhrase()
    {
        IncrementPhrase();
    }

    /// <summary>
    /// ミス時またはリセット時に呼び出し：Phraseリセット
    /// </summary>
    public void ResetPhrase()
    {
        currentPhrase = 0;
        UpdatePhraseDisplay();
    }

    void UpdatePhraseDisplay()
    {
        if (phraseText != null)
        {
            if (currentPhrase >= 5)
                phraseText.text = $"Phrase x{currentPhrase}";
            else
                phraseText.text = "";
        }
    }

    public int GetCurrentPhrase()
    {
        return currentPhrase;
    }

    public int GetMaxPhrase()
    {
        return maxPhrase;
    }
}