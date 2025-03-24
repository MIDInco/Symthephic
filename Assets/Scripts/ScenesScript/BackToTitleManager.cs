using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToTitleManager : MonoBehaviour
{
    // ポーズ画面などから呼び出す
    public void BackToTitle()
    {
        Time.timeScale = 1f;

        // 🎯 ゲーム状態を完全リセット
        GameStateResetter.Reset();

        // 🎯 タイトルへ戻る
        SceneManager.LoadScene("TitleScene");
    }
}   
