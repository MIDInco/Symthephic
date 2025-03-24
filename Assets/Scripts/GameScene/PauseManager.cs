using UnityEngine;
using UnityEngine.SceneManagement;

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

        // ✅ 追加：リスタート処理
    public void RestartGame()
    {
        Debug.Log("🔄 RestartGame関数が呼ばれました");

        Time.timeScale = 1f; // 念のため時間を戻す
        SceneManager.LoadScene("GameScene"); // 同じシーンを再読み込み
    }
}

