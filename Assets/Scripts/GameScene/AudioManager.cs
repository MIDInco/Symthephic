using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource audioSource;
    public event Action OnAudioPlaybackStarted;
    private bool hasAudioStarted = false;

    public AudioMixer audioMixer; // ← 追加！

    private const string VolumeParameter = "MasterVolume"; // ← AudioMixer のパラメータ名

void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
                Debug.Log("🔁 AudioSource を Awake() で復元しました！");
            else
                Debug.LogError("❌ AudioManager: AudioSource が見つかりません！（Awake）");
        }

        // 追加ここから
        Debug.Log($"🎧 AudioManager 初期化: GameObject = {gameObject.name}, InstanceID = {GetInstanceID()}");
        if (audioSource != null)
        {
            Debug.Log($"🎵 AudioSource 状態: clip={audioSource.clip?.name}, isPlaying={audioSource.isPlaying}");
        }
        // 追加ここまで
    }
    else
    {
        Debug.LogWarning("⚠ AudioManager: 重複したインスタンスが生成されました。破棄します。");
        Destroy(gameObject);
    }

    ApplyMasterVolume();
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
    if (MusicManager.SelectedMusic == null)
    {
        Debug.LogError("❌ AudioManager: MusicManager.SelectedSong が null です！");
        return;
    }

    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(MusicManager.SelectedMusic.AudioFileName);
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

    private void ApplyMasterVolume()
{
    float value = GameSettings.MasterVolume;
    float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(value, 0.3f));

    if (audioMixer != null)
    {
        audioMixer.SetFloat(VolumeParameter, db);
        Debug.Log($"🔊 AudioManager: マスターボリュームを適用しました → dB={db}");
    }
    else
    {
        Debug.LogWarning("⚠ AudioManager: audioMixer が未設定のため、音量を適用できません");
    }
}
}
