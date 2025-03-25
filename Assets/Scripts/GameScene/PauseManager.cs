using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public ReadyUIController readyUIController;

    private bool isPaused = false;

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;

        if (readyUIController != null)
        {
            StartCoroutine(readyUIController.PlayReadySequence(() =>
            {
                Time.timeScale = 1f;
                AudioManager.Instance?.audioSource?.UnPause();
                FindAnyObjectByType<ChartPlaybackManager>()?.ResumeChart();
            }));
        }
        else
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.audioSource?.UnPause();
            FindAnyObjectByType<ChartPlaybackManager>()?.ResumeChart();
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        AudioManager.Instance?.audioSource?.Pause();
        FindAnyObjectByType<ChartPlaybackManager>()?.PauseChart();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void BackToMusicSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MusicSelectScene");
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}

