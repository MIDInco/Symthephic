using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void LoadSongSelectScene()
    {
        Debug.Log("🎯 LoadSongSelectScene() が呼ばれました！"); // ✅ メソッドが実行されたことを確認

        if (Application.CanStreamedLevelBeLoaded("SongSelectScene"))
        {
            Debug.Log("✅ SongSelectScene のロードを開始します！");
            SceneManager.LoadScene("SongSelectScene"); // 🎯 シーン遷移
        }
        else
        {
            Debug.LogError("❌ SongSelectScene が Build Settings に追加されていません！");
        }
    }
}
