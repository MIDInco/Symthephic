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
            Debug.LogError("❌ AudioSource がアタッチされていません！");
        }
        else if (audioSource.clip == null)
        {
            Debug.LogWarning("⚠ AudioClip は未設定ですが、後で設定する場合は問題ありません。");
        }
    }

    void Update()
    {
        if (!hasAudioStarted && audioSource != null && audioSource.isPlaying)
        {
            hasAudioStarted = true;
            OnAudioPlaybackStarted?.Invoke();
            Debug.Log($"🎵 オーディオが再生開始されました！ {AudioSettings.dspTime:F3} sec");
        }
    }

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

public void PlaySelectedAudio()
{
    if (SongManager.SelectedSong == null)
    {
        Debug.LogError("❌ AudioManager: SongManager.SelectedSong が null です！");
        return;
    }

    string path = "PlayTest_Audio/" + SongManager.SelectedSong.AudioFileName;
    Debug.Log($"🎵 AudioManager: {path} からオーディオをロード");

    AudioClip clip = Resources.Load<AudioClip>(path);

    if (clip == null)
    {
        Debug.LogError($"❌ AudioManager: オーディオファイルが見つかりません: {path}");
        return;
    }

    audioSource.clip = clip;
    hasAudioStarted = false;
    Debug.Log($"✅ AudioManager: オーディオロード成功！{SongManager.SelectedSong.AudioFileName}");

    // ✅ 確実に再生されるかチェック
    audioSource.Play();
    Debug.Log($"▶ AudioManager: {SongManager.SelectedSong.AudioFileName} の再生開始！");
}

}
