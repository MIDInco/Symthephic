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
            DontDestroyOnLoad(gameObject); // シーンをまたいでもAudioManagerを保持
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
            Debug.LogError("❌ AudioSource がアタッチされていません！");
        }
        else if (audioSource.clip == null)
        {
            Debug.LogWarning("⚠ AudioClip は未設定ですが、後で設定する場合は問題ありません。");
        }
    }

    void Update()
    {
        // 再生が始まった瞬間だけイベントを発火（1回のみ）
        if (!hasAudioStarted && audioSource != null && audioSource.isPlaying)
        {
            hasAudioStarted = true;
            OnAudioPlaybackStarted?.Invoke();
            Debug.Log($"🎵 オーディオが再生開始されました！ {AudioSettings.dspTime:F3} sec");
        }
    }

    // 🎯 現在のAudioClipを即時再生
    public void PlayAudioNow()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("❌ AudioSource または AudioClip が設定されていません！");
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log($"✅ オーディオを即時再生！ (時間: {AudioSettings.dspTime:F3})");
            OnAudioPlaybackStarted?.Invoke();
        }
        else
        {
            Debug.Log("⚠ 既にオーディオが再生中です。");
        }
    }

    // 🎯 指定秒数後に再生予約
    public void PlayAudioWithDelay(float delaySeconds)
    {
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("❌ AudioSource または AudioClip が設定されていません！");
            return;
        }

        if (!audioSource.isPlaying)
        {
            double playTime = AudioSettings.dspTime + delaySeconds;
            audioSource.PlayScheduled(playTime);
            Debug.Log($"✅ {playTime:F3} sec にオーディオを再生予定 (現在時刻 {AudioSettings.dspTime:F3})");
        }
        else
        {
            Debug.Log("⚠ 既にオーディオが再生中です。");
        }
    }

    // 🎯 SongManager.SelectedSong からオーディオファイルを読み込んで再生
    public void PlaySelectedAudio()
    {
        if (SongManager.SelectedSong == null)
        {
            Debug.LogError("❌ 選択された曲情報が存在しません！");
            return;
        }

        string path = "Audio/" + SongManager.SelectedSong.AudioFileName;
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip == null)
        {
            Debug.LogError($"❌ オーディオファイルが見つかりません: {path}（Resources以下に配置されていますか？）");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        hasAudioStarted = true; // 再生開始を通知させる
        OnAudioPlaybackStarted?.Invoke();
        Debug.Log($"✅ オーディオ再生開始: {SongManager.SelectedSong.AudioFileName}");
    }
}
