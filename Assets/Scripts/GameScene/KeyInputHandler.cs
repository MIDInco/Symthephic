using UnityEngine;

public class KeyInputHandler : MonoBehaviour
{
    public JudgmentManager judgmentManager; // 修正: JudgmentManager に処理を送る

    void Update()
    {
       // if (Input.anyKeyDown) Debug.Log("キーが押されました！");

        if (Input.GetKeyDown(KeyCode.S))
            RegisterKeyPress(60); // C4
        if (Input.GetKeyDown(KeyCode.D))
            RegisterKeyPress(61); // C#4
        if (Input.GetKeyDown(KeyCode.F))
            RegisterKeyPress(62); // D4
        if (Input.GetKeyDown(KeyCode.J))
            RegisterKeyPress(63); // D#4
        if (Input.GetKeyDown(KeyCode.K))
            RegisterKeyPress(64); // E4
        if (Input.GetKeyDown(KeyCode.L))
            RegisterKeyPress(65); // F4

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameSceneManager.Instance != null)
            {
            if (GameSceneManager.IsPaused)
                GameSceneManager.Instance.ResumeGame();
            else
                GameSceneManager.Instance.PauseGame();
            }
        }
    }

    void RegisterKeyPress(int noteValue)
    {
        if (judgmentManager != null)
        {
            judgmentManager.ProcessKeyPress(noteValue); // 修正: NotesGenerator ではなく JudgmentManager に処理を送る
            Debug.Log($"Key Pressed: Note {noteValue}");
        }
        else
        {
            Debug.LogError("❌ JudgmentManager が設定されていません！");
        }
    }
}