using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToTitleManager : MonoBehaviour
{
    // ポーズ画面などから呼び出す
    public void BackToTitle()
    {
        Time.timeScale = 1f; // 念のためゲームが止まっていたら解除
        SceneManager.LoadScene("TitleScene"); // タイトルシーンの名前に合わせて変更
    }
}
