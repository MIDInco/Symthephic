using UnityEngine;

public class KeyInputHandler : MonoBehaviour
{
    public JudgmentManager judgmentManager;
    public ReadyUIController readyUIController; // 追加

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) RegisterKeyPress(60);
        if (Input.GetKeyDown(KeyCode.D)) RegisterKeyPress(61);
        if (Input.GetKeyDown(KeyCode.F)) RegisterKeyPress(62);
        if (Input.GetKeyDown(KeyCode.J)) RegisterKeyPress(63);
        if (Input.GetKeyDown(KeyCode.K)) RegisterKeyPress(64);
        if (Input.GetKeyDown(KeyCode.L)) RegisterKeyPress(65);

        if (Input.GetKeyUp(KeyCode.S)) RegisterKeyRelease(60);
        if (Input.GetKeyUp(KeyCode.D)) RegisterKeyRelease(61);
        if (Input.GetKeyUp(KeyCode.F)) RegisterKeyRelease(62);
        if (Input.GetKeyUp(KeyCode.J)) RegisterKeyRelease(63);
        if (Input.GetKeyUp(KeyCode.K)) RegisterKeyRelease(64);
        if (Input.GetKeyUp(KeyCode.L)) RegisterKeyRelease(65);

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

    void RegisterKeyRelease(int noteValue)
    {
        if (GameSceneManager.IsPaused || GameSceneManager.IsResuming)
            return;

        if (judgmentManager != null)
        {
            judgmentManager.ProcessKeyRelease(noteValue);
            Debug.Log($"Key Released: Note {noteValue}");
        }
        else
        {
            Debug.LogError("❌ JudgmentManager が設定されていません！");
        }
    }

    void HandlePauseToggle()
    {
        if (GameSceneManager.Instance == null) return;

        if (readyUIController != null && readyUIController.IsPlayingReadySequence)
        {
            Debug.Log("⏸ Ready中のためポーズできません");
            return;
        }

        // カウントダウン中もポーズ不可
        if (CountdownManager.IsCountingDown)
        {
            Debug.Log("⏸ カウントダウン中のためポーズできません");
            return;
        }

        PauseManager pauseManager = FindAnyObjectByType<PauseManager>();
        if (pauseManager == null)
        {
            Debug.LogWarning("❌ PauseManager が見つかりません！");
            return;
        }

        if (GameSceneManager.IsPaused)
            pauseManager.Resume();
        else
            GameSceneManager.Instance.PauseGame();
    }
}

