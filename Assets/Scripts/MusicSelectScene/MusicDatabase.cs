using System.Collections.Generic;
using UnityEngine;
using System;

public static class MusicDatabase
{
    private static Dictionary<string, MusicData> musicLookup = new Dictionary<string, MusicData>();

    static MusicDatabase()
    {
        LoadFromJson();
    }

    private static void LoadFromJson()
    {
        TextAsset json = Resources.Load<TextAsset>("MusicDatabase");
        if (json == null)
        {
            Debug.LogError("❌ Resources/MusicDatabase.json が見つかりません！");
            return;
        }

        try
        {
            string wrappedJson = "{\"musics\":" + json.text + "}";
            MusicDataListWrapper wrapper = JsonUtility.FromJson<MusicDataListWrapper>(wrappedJson);
            foreach (var music in wrapper.musics)
            {
                musicLookup[music.MidiFileName] = music;
            }

            Debug.Log($"📦 MusicDatabase: {musicLookup.Count} 曲を読み込みました");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ MusicDatabase の読み込みに失敗しました: " + e.Message);
        }
    }

    [Serializable]
    private class MusicDataListWrapper
    {
        public MusicData[] musics;
    }

    public static MusicData GetMusicData(string midiFileName)
    {
        if (musicLookup.TryGetValue(midiFileName, out var music))
            return music;

        return new MusicData(midiFileName, midiFileName, midiFileName + ".wav");
    }

    public static List<MusicData> GetAllMusics()
    {
        return new List<MusicData>(musicLookup.Values);
    }
}
