using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
void Start()
{
    Debug.Log("🎯 GameSceneManager: Start() 実行");

    if (SongManager.SelectedSong == null)
    {
        Debug.LogError("❌ GameSceneManager: SongManager.SelectedSong が null です！");
        return;
    }

    Debug.Log($"🎶 選択された曲: {SongManager.SelectedSong.DisplayName} (MIDI: {SongManager.SelectedSong.MidiFileName}, Audio: {SongManager.SelectedSong.AudioFileName})");

    // ✅ オーディオの読み込み
    AudioManager.Instance.PlaySelectedAudio();

    // ✅ MIDI譜面の読み込み
    NotesGenerator generator = FindObjectOfType<NotesGenerator>();
    if (generator != null)
    {
        generator.LoadSelectedMidiAndGenerateNotes();
    }
}
}
