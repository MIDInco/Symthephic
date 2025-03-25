using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void LoadMusicSelectScene()
    {
        Debug.Log("🎯 LoadMusicSelectScene() が呼ばれました！");

        if (Application.CanStreamedLevelBeLoaded("MusicSelectScene"))
        {
            Debug.Log("✅ MusicSelectScene のロードを開始します！");
            SceneManager.LoadScene("MusicSelectScene");
        }
        else
        {
            Debug.LogError("❌ MusicSelectScene が Build Settings に追加されていません！");
        }
    }

    public void LoadResultScene()
    {
        Debug.Log("🎯 LoadResultScene() が呼ばれました！");

        if (Application.CanStreamedLevelBeLoaded("ResultScene"))
        {
            Debug.Log("✅ ResultScene のロードを開始します！");
            SceneManager.LoadScene("ResultScene");
        }
        else
        {
            Debug.LogError("❌ ResultScene が Build Settings に追加されていません！");
        }
    }

    public void LoadGameScene()
    {
        Debug.Log("🎯 LoadGameScene() が呼ばれました！");

        if (Application.CanStreamedLevelBeLoaded("GameScene"))
        {
            Debug.Log("✅ GameScene のロードを開始します！");
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("❌ GameScene が Build Settings に追加されていません！");
        }
    }
}
