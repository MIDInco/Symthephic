using System.Collections.Generic;
using UnityEngine;

public static class MusicManager
{
    public static List<MusicData> AvailableMusics = new List<MusicData>();
    public static MusicData SelectedMusic = null;

    // 🎯 選択した曲をセット
    public static void SetSelectedMusic(string midiFileName)
    {
        SelectedMusic = AvailableMusics.Find(music => music.MidiFileName == midiFileName);
        if (SelectedMusic != null)
        {
            Debug.Log($"✅ 選択した曲: {SelectedMusic.DisplayName} / MIDI: {SelectedMusic.MidiFileName} / Audio: {SelectedMusic.AudioFileName}");
        }
        else
        {
            Debug.LogError($"❌ 選択したMIDIがリストにありません: {midiFileName}");
        }
    }

    public static string GetSelectedAudioFile()
    {
        return SelectedMusic?.AudioFileName;
    }
}
