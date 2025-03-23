using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // ポーズ用UI（ボタン含む Canvas）

    private bool isPaused = false;

void Update()
{
    //Debug.Log("Update called"); // ← これを追加
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
        Debug.Log("Resume関数が呼ばれました"); // ← ここも追加（おまけ）
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        Debug.Log("Pause関数が呼ばれました"); // ← ここを追加
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
}

