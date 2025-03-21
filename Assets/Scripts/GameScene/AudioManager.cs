using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource audioSource;
    public event Action OnAudioPlaybackStarted;
    private bool hasAudioStarted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("❌ AudioManager: AudioSource がアタッチされていません！");
        }
    }

   public void PlaySelectedAudio()
{
    if (SongManager.SelectedSong == null)
    {
        Debug.LogError("❌ AudioManager: SongManager.SelectedSong が null です！");
        return;
    }

    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(SongManager.SelectedSong.AudioFileName);
    string path = "Playlist_Audio/" + fileNameWithoutExtension;

    Debug.Log($"🎵 AudioManager: {path} からオーディオをロード");

    AudioClip clip = Resources.Load<AudioClip>(path);

    if (clip == null)
    {
        Debug.LogError($"❌ AudioManager: オーディオファイルが見つかりません: {path}");
        DebugResourcesAudioFiles();
        return;
    }

    audioSource.clip = clip;
    hasAudioStarted = false;
    Debug.Log($"✅ AudioManager: オーディオロード成功！{fileNameWithoutExtension}");

    // 🚀 ここでの `audioSource.Play();` を削除する
}


    // ✅ 追加: Resources/Playlist_Audio 内のオーディオファイル一覧をデバッグ出力
    private void DebugResourcesAudioFiles()
    {
        Debug.Log("📂 AudioManager: Resources.LoadAll<AudioClip>(\"Playlist_Audio\") で取得できるファイル一覧:");
        AudioClip[] allClips = Resources.LoadAll<AudioClip>("Playlist_Audio");
        foreach (var clip in allClips)
        {
            Debug.Log($" - {clip.name}");
        }
    }
}
