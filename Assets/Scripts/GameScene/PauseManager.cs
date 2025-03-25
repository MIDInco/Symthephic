using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // ポーズ用UI（ボタン含む Canvas）
    public ReadyUIController readyUIController; // ✅ Ready演出を制御するコンポーネント（インスペクタで指定）

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
public void Resume()
{
    Debug.Log("Resume関数が呼ばれました");

    pauseMenuUI.SetActive(false);
    isPaused = false;

    // ✅ Readyが終わったら Time.timeScale を戻す
    if (readyUIController != null)
    {
        StartCoroutine(readyUIController.PlayReadySequence(() =>
        {
            Time.timeScale = 1f;
            Debug.Log("▶ PauseManager: Ready終了後に Time.timeScale = 1f に戻しました");
        }));
    }
    else
    {
        Debug.LogWarning("⚠ ReadyUIController が未設定のため、即再開");
        Time.timeScale = 1f;
        AudioManager.Instance?.audioSource?.UnPause();
        FindAnyObjectByType<ChartPlaybackManager>()?.ResumeChart();
    }
}


    public void Pause()
    {
        Debug.Log("Pause関数が呼ばれました");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        if (AudioManager.Instance?.audioSource?.isPlaying == true)
        {
            AudioManager.Instance.audioSource.Pause();
            Debug.Log("🔇 AudioManager: AudioをPauseしました");
        }

        FindAnyObjectByType<ChartPlaybackManager>()?.PauseChart();
    }

    public void RestartGame()
    {
        Debug.Log("🔄 RestartGame関数が呼ばれました");
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }
}
