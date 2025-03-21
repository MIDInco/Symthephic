using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MIDIファイル名に対応する本当の曲名やオーディオファイル名を管理するデータベース
/// </summary>
public static class SongDatabase
{
    private static Dictionary<string, SongData> songLookup = new Dictionary<string, SongData>
    {
        //MIDI名、MIDI名、実際の名前、オーディオファイル名
        { "0001_Kousaku", new SongData("0001_Kousaku", "交錯", "Kousaku_Master2mix.wav") },
        { "0003_Taihaiteki_Hard", new SongData("0003_Taihaiteki_Hard", "退廃的快楽物質", "Taihaitekikairakubussitsu.wav") },
        //{ "0004_Kousaku_Test", new SongData("0004_Kousaku_Test", "交錯_Test", "Kousaku_Test.wav") },
        //{ "20250319_Test_120_noAttack", new SongData("20250319_Test_120_noAttack", "120", "Test_01.wav") },
        //{ "20250320_Test_90", new SongData("20250320_Test_90", "90", "20250320_Test_90.wav") },
        //{ "Test_150", new SongData("Test_150", "てすと", "150_Test.wav") }
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
