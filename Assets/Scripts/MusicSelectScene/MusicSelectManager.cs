using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MidiPlayerTK;

public class MusicSelectManager : MonoBehaviour
{
    public GameObject musicButtonPrefab; // 🎯 ボタンのプレハブ
    public Transform musicListParent;    // 🎯 ScrollView のコンテナ
    public MidiFilePlayer midiPlayer;    // 🎯 MPTKのMIDIプレイヤー

    void Start()
    {

        Debug.Log("🟢 MusicSelectManager.Start() が呼ばれました");

        if (AudioManager.Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("GameScenes/AudioManager");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("🎶 AudioManager を MusicSelectScene で生成しました！");
            }
            else
            {
                Debug.LogError("❌ AudioManager プレハブが Resources/GameScenes に見つかりません！");
            }
        }

        LoadAvailableMusics();
        DisplayMusicList();
    }

    // 🎯 楽曲リストを読み込む
    void LoadAvailableMusics()
{
    Debug.Log("🔄 LoadAvailableMusics 開始");

    MusicManager.AvailableMusics.Clear();

    foreach (var file in MidiPlayerGlobal.MPTK_ListMidi)
    {
        Debug.Log($"🎼 MIDIラベル: {file.Label}");

        var musicData = MusicDatabase.GetMusicData(file.Label);

        if (musicData == null)
        {
            Debug.LogWarning($"⚠️ '{file.Label}' は MusicDatabase に見つかりません");
            continue;
        }

        Debug.Log($"✅ 登録された曲: {musicData.DisplayName}");
        MusicManager.AvailableMusics.Add(musicData);
    }

    Debug.Log($"🎯 登録された曲の数: {MusicManager.AvailableMusics.Count}");
}


    // 🎯 楽曲リストのUIを生成
    void DisplayMusicList()
    {
    Debug.Log($"🧾 ボタン生成開始。曲数: {MusicManager.AvailableMusics.Count}");

    foreach (var music in MusicManager.AvailableMusics)
    {
        Debug.Log($"🔘 ボタン生成中: {music.DisplayName}");

        GameObject musicButton = Instantiate(musicButtonPrefab, musicListParent);

        var buttonText = musicButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
            buttonText.text = music.DisplayName;
        else
            Debug.LogError("❌ TextMeshProUGUI が見つかりません！");

        musicButton.GetComponent<Button>().onClick.AddListener(() => SelectMusic(music.MidiFileName));
    }
    }


    // 🎯 選択した曲をセットし、GameSceneへ移動
    void SelectMusic(string midiFileName)
    {
        MusicManager.SetSelectedMusic(midiFileName);

        Debug.Log($"🎯 選択されたMIDI: {MusicManager.SelectedMusic?.MidiFileName} / {MusicManager.SelectedMusic?.DisplayName}");

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
