using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        AudioManager.Instance?.audioSource?.Pause();
        FindAnyObjectByType<ChartPlaybackManager>()?.PauseChart();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // UIだけ閉じる
        GameSceneManager.Instance?.ResumeGame(); // Ready演出＋再開処理はここに任せる
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
