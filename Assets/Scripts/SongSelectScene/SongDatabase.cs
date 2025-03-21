using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MIDIファイル名に対応する本当の曲名やオーディオファイル名を管理するデータベース
/// </summary>
public static class SongDatabase
{
    private static Dictionary<string, SongData> songLookup = new Dictionary<string, SongData>
    {
        { "Kousaku", new SongData("Kousaku", "交錯", "Kousaku.wav") },
        { "Test_Song", new SongData("Test_Song", "テスト曲", "Test_Song.wav") },
        { "DemoTrack", new SongData("DemoTrack", "デモトラック", "DemoTrack.wav") }
    };

    public static SongData GetSongData(string midiFileName)
    {
        if (songLookup.ContainsKey(midiFileName))
        {
            return songLookup[midiFileName];
        }
        else
        {
            // デフォルトでファイル名そのままを返す
            return new SongData(midiFileName, midiFileName, midiFileName + ".wav");
        }
    }

    public static List<SongData> GetAllSongs()
    {
        return new List<SongData>(songLookup.Values);
    }
}
