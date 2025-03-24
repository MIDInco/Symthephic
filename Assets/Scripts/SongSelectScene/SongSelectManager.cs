using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MidiPlayerTK;

public class SongSelectManager : MonoBehaviour
{
    public GameObject songButtonPrefab; // 🎯 ボタンのプレハブ
    public Transform songListParent;    // 🎯 ScrollView のコンテナ
    public MidiFilePlayer midiPlayer;   // 🎯 MPTKのMIDIプレイヤー

void Start()
{
    if (AudioManager.Instance == null)
    {
        GameObject prefab = Resources.Load<GameObject>("GameScenes/AudioManager");
        if (prefab != null)
        {
            Instantiate(prefab);
            Debug.Log("🎶 AudioManager を SongSelectScene で生成しました！");
        }
        else
        {
            Debug.LogError("❌ AudioManager プレハブが Resources/GameScenes に見つかりません！");
        }
    }

    LoadAvailableSongs();
    DisplaySongList();
}

    // 🎯 楽曲リストを読み込む
void LoadAvailableSongs()
{
    SongManager.AvailableSongs.Clear(); // ← 追加！
    List<MPTKListItem> midiFiles = MidiPlayerGlobal.MPTK_ListMidi;

    foreach (var file in midiFiles)
    {
        // songLookup に存在する場合だけ取得する
        SongData songData = SongDatabase.GetSongData(file.Label);

        // デフォルト生成された曲（＝DisplayNameがMIDIファイル名と同じ）は除外
        if (songData.DisplayName == file.Label)
        {
            Debug.Log($"🛑 {file.Label} は登録されていないのでスキップ");
            continue;
        }

        SongManager.AvailableSongs.Add(songData);
    }

    Debug.Log($"🎵 読み込んだ曲の数: {SongManager.AvailableSongs.Count}");
}



    // 🎯 楽曲リストのUIを生成
    void DisplaySongList()
    {
        foreach (var song in SongManager.AvailableSongs)
        {
            GameObject songButton = Instantiate(songButtonPrefab, songListParent);
            TextMeshProUGUI buttonText = songButton.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = $"{song.DisplayName}"; // 🎯 本当の曲名を表示
            }
            else
            {
                Debug.LogError("❌ TextMeshProUGUI が見つかりません！");
            }

            songButton.GetComponent<Button>().onClick.AddListener(() => SelectSong(song.MidiFileName));
        }
    }

    // 🎯 選択した曲をセットし、GameSceneへ移動
void SelectSong(string midiFileName)
{
    SongManager.SetSelectedSong(midiFileName);
    
    // ✅ 選択された曲をログに出力
    Debug.Log($"🎯 選択されたMIDI: {SongManager.SelectedSong?.MidiFileName} / {SongManager.SelectedSong?.DisplayName}");

    // 🎯 GameScene へ遷移
    UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
}

}
