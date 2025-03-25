using UnityEngine;

public class KeyInputHandler : MonoBehaviour
{
    public JudgmentManager judgmentManager; // 修正: JudgmentManager に処理を送る

void Update()
{
    if (Input.GetKeyDown(KeyCode.S)) RegisterKeyPress(60);
    if (Input.GetKeyDown(KeyCode.D)) RegisterKeyPress(61);
    if (Input.GetKeyDown(KeyCode.F)) RegisterKeyPress(62);
    if (Input.GetKeyDown(KeyCode.J)) RegisterKeyPress(63);
    if (Input.GetKeyDown(KeyCode.K)) RegisterKeyPress(64);
    if (Input.GetKeyDown(KeyCode.L)) RegisterKeyPress(65);

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        HandlePauseToggle();
    }
}

void RegisterKeyPress(int noteValue)
{
    if (GameSceneManager.IsPaused || GameSceneManager.IsResuming)
        return;

    if (judgmentManager != null)
    {
        judgmentManager.ProcessKeyPress(noteValue);
        Debug.Log($"Key Pressed: Note {noteValue}");
    }
    else
    {
        Debug.LogError("❌ JudgmentManager が設定されていません！");
    }
}

void HandlePauseToggle()
{
    if (GameSceneManager.Instance == null) return;

    if (GameSceneManager.IsPaused)
        GameSceneManager.Instance.ResumeGame();
    else
        GameSceneManager.Instance.PauseGame();
}
}