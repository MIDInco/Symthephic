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
        LoadAvailableSongs();
        DisplaySongList();
    }

    // 🎯 楽曲リストを読み込む
void LoadAvailableSongs()
{
    List<MPTKListItem> midiFiles = MidiPlayerGlobal.MPTK_ListMidi;

    Debug.Log("🎵 MPTKに登録されているMIDIリスト:");
    foreach (var file in midiFiles)
    {
        Debug.Log($" - {file.Label}");
    }

    foreach (var file in midiFiles)
    {
        SongData songData = SongDatabase.GetSongData(file.Label);
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
