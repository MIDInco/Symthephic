using System.Collections.Generic;
using UnityEngine;
using System;

public static class SongDatabase
{
    private static Dictionary<string, SongData> songLookup = new Dictionary<string, SongData>();

    static SongDatabase()
    {
        LoadFromJson();
    }

    private static void LoadFromJson()
    {
        TextAsset json = Resources.Load<TextAsset>("SongDatabase");
        if (json == null)
        {
            Debug.LogError("❌ Resources/SongDatabase.json が見つかりません！");
            return;
        }

        try
        {
            string wrappedJson = "{\"songs\":" + json.text + "}";
            SongDataListWrapper wrapper = JsonUtility.FromJson<SongDataListWrapper>(wrappedJson);
            foreach (var song in wrapper.songs)
            {
                songLookup[song.MidiFileName] = song;
            }

            Debug.Log($"📦 SongDatabase: {songLookup.Count} 曲を読み込みました");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ SongDatabase の読み込みに失敗しました: " + e.Message);
        }
    }

    [Serializable]
    private class SongDataListWrapper
    {
        public SongData[] songs;
    }

    public static SongData GetSongData(string midiFileName)
    {
        if (songLookup.TryGetValue(midiFileName, out var song))
            return song;

        return new SongData(midiFileName, midiFileName, midiFileName + ".wav");
    }

    public static List<SongData> GetAllSongs()
    {
        return new List<SongData>(songLookup.Values);
    }
}
