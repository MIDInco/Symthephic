using UnityEngine;

public class GameExit : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("ゲームを終了します");
        Application.Quit();

        // エディタ上で確認する場合（ビルドしたら無効になる）
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
