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
        DontDestroyOnLoad(gameObject); // ✅ 全シーン共通で使う

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
                Debug.Log("🔁 AudioSource を Awake() で復元しました！");
            else
                Debug.LogError("❌ AudioManager: AudioSource が見つかりません！（Awake）");
        }
    }
    else
    {
        Destroy(gameObject); // ✅ 二重生成を防ぐ
    }
}

void Start()
{
    if (audioSource == null)
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("❌ AudioManager: AudioSource が見つかりません！（Start時）");
        }
        else
        {
            Debug.Log("🔁 AudioSource を復元しました！");
        }
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

    if (audioSource == null)
    {
        Debug.LogError("❌ AudioManager: audioSource が null のため、オーディオを再生できません！");
        return;
    }

    audioSource.clip = clip;

    // 🎯 再生位置リセット（これが今回の修正点！）
    audioSource.time = 0f;             // 秒単位で0にリセット
    // audioSource.timeSamples = 0;   // サンプル単位でリセットしたい場合はこちら（高精度）

    hasAudioStarted = false;
    Debug.Log($"✅ AudioManager: オーディオロード成功！{fileNameWithoutExtension}");

    // 🚫 Play はここでは呼ばない（ChartPlaybackManagerが制御）
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
