using System.Collections.Generic;
using UnityEngine;

public static class SongManager
{
    public static List<SongData> AvailableSongs = new List<SongData>();
    public static SongData SelectedSong = null;

    // 🎯 選択した曲をセット
    public static void SetSelectedSong(string midiFileName)
    {
        SelectedSong = AvailableSongs.Find(song => song.MidiFileName == midiFileName);
        if (SelectedSong != null)
        {
            Debug.Log($"✅ 選択した曲: {SelectedSong.DisplayName} / MIDI: {SelectedSong.MidiFileName} / Audio: {SelectedSong.AudioFileName}");
        }
        else
        {
            Debug.LogError($"❌ 選択したMIDIがリストにありません: {midiFileName}");
        }
    }

    public static string GetSelectedAudioFile()
{
    return SelectedSong?.AudioFileName;
}
}
