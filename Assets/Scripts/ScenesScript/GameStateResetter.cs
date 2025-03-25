using UnityEngine;

public static class GameStateResetter
{
    public static void Reset()
    {
        Debug.Log("🔄 GameStateResetter: ゲーム状態を初期化します");

        // ✅ NotesGenerator のノートと isReady をリセット
        NotesGenerator notesGen = GameObject.FindFirstObjectByType<NotesGenerator>();
        if (notesGen != null)
        {
            notesGen.ResetState(); // ← さっき追加したメソッド
        }

            // ✅ ScoreManager と PhraseManager のリセット ← ここを追加！
        ScoreManager score = GameObject.FindFirstObjectByType<ScoreManager>();
        score?.ResetScore();

        PhraseManager phrase = GameObject.FindFirstObjectByType<PhraseManager>();
        phrase?.ResetPhrase();

        // ✅ MusicManager の状態をリセット
        MusicManager.SelectedMusic = null;
        MusicManager.AvailableMusics.Clear();

        // ✅ AudioManager を削除（DontDestroyOnLoad対策）
        if (AudioManager.Instance != null)
        {
            GameObject.Destroy(AudioManager.Instance.gameObject);
            Debug.Log("🗑 AudioManager を削除しました");
        }

        // ✅ Noteoffset が Singleton なら削除
        if (Noteoffset.Instance != null)
        {
            GameObject.Destroy(Noteoffset.Instance.gameObject);
            Debug.Log("🗑 Noteoffset を削除しました");
        }

        

        // ✅ GameSettings のリセット（必要に応じて）
        //GameSettings.NoteSpeed = 5.0f;
        //GameSettings.MasterVolume = 0.8f;

        Debug.Log("✅ ゲーム状態の初期化が完了しました");
    }
}
