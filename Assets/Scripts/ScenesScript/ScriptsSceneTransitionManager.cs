using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void LoadMusicSelectScene()
    {
        Debug.Log("🎯 LoadMusicSelectScene() が呼ばれました！"); // ✅ メソッドが実行されたことを確認

        if (Application.CanStreamedLevelBeLoaded("MusicSelectScene"))
        {
            Debug.Log("✅ MusicSelectScene のロードを開始します！");
            SceneManager.LoadScene("MusicSelectScene"); // 🎯 シーン遷移
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
}
