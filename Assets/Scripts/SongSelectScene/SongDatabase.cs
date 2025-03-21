using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MIDIファイル名に対応する本当の曲名やオーディオファイル名を管理するデータベース
/// </summary>
public static class SongDatabase
{
    private static Dictionary<string, SongData> songLookup = new Dictionary<string, SongData>
    {
        { "0001_Kousaku", new SongData("0001_Kousaku", "交錯", "Kousaku_Master2mix.wav") },
        { "Test_Song", new SongData("Test_Song", "テスト曲", "Test_Song.wav") },
        { "20250320_Test_90", new SongData("20250320_Test_90", "90", "20250320_Test_90.wav") }
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
